using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.Models.Enums;
using PharMarket.ViewModels.POS;
using PharMarket.ViewModels.Sales;

namespace PharMarket.Services;

public class SalesService : ISalesService
{
    private readonly AppDbContext _context;

    public SalesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> ProcessSaleAsync(ProcessSaleViewModel model, int storeId, int? userId = null)
    {
        if (!model.Items.Any())
            throw new BadRequestException("Sale must contain at least one item.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal subTotal = 0;
            decimal taxAmount = 0;

            var saleItems = new List<SaleItem>();

            foreach (var item in model.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Stock)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.StoreId == storeId)
                    ?? throw new NotFoundException("Product", item.ProductId);

                if (product.Stock == null || (product.Stock.StoreQuantity + product.Stock.ShelfQuantity) < item.Quantity)
                    throw new BadRequestException($"Insufficient stock for {product.Name}.");

                var itemTotal = item.UnitPrice * item.Quantity;
                var itemTax = itemTotal * product.TaxRate / 100;

                subTotal += itemTotal;
                taxAmount += itemTax;

                saleItems.Add(new SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CostPrice = product.CostPrice,
                    Total = itemTotal
                });

                var toDeduct = item.Quantity;
                if (product.Stock.ShelfQuantity >= toDeduct)
                {
                    product.Stock.ShelfQuantity -= toDeduct;
                }
                else
                {
                    toDeduct -= product.Stock.ShelfQuantity;
                    product.Stock.ShelfQuantity = 0;
                    product.Stock.StoreQuantity -= toDeduct;
                }

                // Check for low stock notification
                var remaining = (product.Stock.StoreQuantity + product.Stock.ShelfQuantity);
                if (remaining <= product.MinimumStock && remaining > 0)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StoreId = storeId,
                        Title = "Low Stock Alert",
                        Message = $"{product.Name} is running low. Only {remaining} left in stock.",
                        Type = "warning",
                        ProductId = product.Id
                    });
                }
                else if (remaining == 0)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StoreId = storeId,
                        Title = "Out of Stock",
                        Message = $"{product.Name} is now out of stock!",
                        Type = "danger",
                        ProductId = product.Id
                    });
                }
            }

            var totalAmount = subTotal + taxAmount;
            var change = model.PaymentMethod == PaymentMethod.Cash
                ? Math.Max(0, model.AmountPaid - totalAmount)
                : 0;

            var sale = new Sale
            {
                InvoiceNumber = GenerateInvoiceNumber(),
                SaleDate = DateTime.UtcNow,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaymentMethod = model.PaymentMethod,
                AmountPaid = model.AmountPaid,
                ChangeGiven = change,
                CashierName = model.CashierName,
                StoreId = storeId,
                UserId = userId,
                SaleItems = saleItems
            };

            _context.Sales.Add(sale);

            _context.Transactions.Add(new Transaction
            {
                Type = TransactionType.Sale,
                Amount = totalAmount,
                Direction = TransactionDirection.Credit,
                PaymentMethod = model.PaymentMethod,
                Description = $"Sale - {sale.InvoiceNumber}",
                TransactionDate = DateTime.UtcNow,
                RunningBalance = await CalculateRunningBalanceAsync(storeId) + totalAmount,
                StoreId = storeId
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SalesReportViewModel> GetSalesReportAsync(int storeId, DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.StoreId == storeId);

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value.AddDays(1));

        if (paymentMethod.HasValue)
            query = query.Where(s => s.PaymentMethod == paymentMethod.Value);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SalesReportItem
            {
                InvoiceNumber = s.InvoiceNumber,
                SaleDate = s.SaleDate,
                SubTotal = s.SubTotal,
                TaxAmount = s.TaxAmount,
                TotalAmount = s.TotalAmount,
                PaymentMethod = s.PaymentMethod,
                CashierName = s.CashierName,
                ItemCount = s.SaleItems.Count
            })
            .ToListAsync();

        return new SalesReportViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            PaymentMethodFilter = paymentMethod,
            Sales = sales
        };
    }

    public async Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int saleId, int storeId)
    {
        return await _context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .AsNoTracking()
            .Where(s => s.Id == saleId && s.StoreId == storeId)
            .Select(s => new SaleDetailsViewModel
            {
                InvoiceNumber = s.InvoiceNumber,
                SaleDate = s.SaleDate,
                SubTotal = s.SubTotal,
                TaxAmount = s.TaxAmount,
                TotalAmount = s.TotalAmount,
                PaymentMethod = s.PaymentMethod,
                AmountPaid = s.AmountPaid,
                ChangeGiven = s.ChangeGiven,
                CashierName = s.CashierName,
                Items = s.SaleItems.Select(si => new SaleItemDetail
                {
                    ProductName = si.Product.Name,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    Total = si.Total
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<Sale>> GetDailySalesSummaryAsync(DateTime date, int storeId)
    {
        return await _context.Sales
            .Where(s => s.SaleDate.Date == date.Date && !s.IsDeleted && s.StoreId == storeId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    private static string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private async Task<decimal> CalculateRunningBalanceAsync(int storeId)
    {
        var lastTransaction = await _context.Transactions
            .Where(t => t.StoreId == storeId)
            .OrderByDescending(t => t.TransactionDate)
            .FirstOrDefaultAsync();

        return lastTransaction?.RunningBalance ?? 0;
    }
}
