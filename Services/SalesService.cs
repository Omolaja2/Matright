using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
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
                Notes = model.Notes,
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

    public async Task<(SalesReportViewModel Model, int TotalCount)> GetSalesReportAsync(int storeId, DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod, int page = 1, int pageSize = 20)
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

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SalesReportItem
            {
                SaleId = s.Id,
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

        return (new SalesReportViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            PaymentMethodFilter = paymentMethod,
            Sales = sales
        }, totalCount);
    }

    public async Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int saleId, int storeId)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .AsNoTracking()
            .Where(s => s.Id == saleId && s.StoreId == storeId)
            .Select(s => new SaleDetailsViewModel
            {
                SaleId = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                SaleDate = s.SaleDate,
                SubTotal = s.SubTotal,
                TaxAmount = s.TaxAmount,
                TotalAmount = s.TotalAmount,
                PaymentMethod = s.PaymentMethod,
                AmountPaid = s.AmountPaid,
                ChangeGiven = s.ChangeGiven,
                CashierName = s.CashierName,
                Notes = s.Notes,
                Items = s.SaleItems.Select(si => new SaleItemDetail
                {
                    ProductName = si.Product.Name,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    CostPrice = si.CostPrice,
                    Total = si.Total
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (sale == null) return null;

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var yesterdayStart = todayStart.AddDays(-1);
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var lastWeekStart = weekStart.AddDays(-7);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todaySalesTotal = await _context.Sales
            .Where(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= todayStart && s.SaleDate < todayStart.AddDays(1))
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var yesterdaySalesTotal = await _context.Sales
            .Where(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= yesterdayStart && s.SaleDate < todayStart)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var thisWeekSalesTotal = await _context.Sales
            .Where(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= weekStart)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var lastWeekSalesTotal = await _context.Sales
            .Where(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= lastWeekStart && s.SaleDate < weekStart)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var monthSalesTotal = await _context.Sales
            .Where(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= monthStart)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var bestSellingThisWeek = await _context.SaleItems
            .Where(si => si.Sale.StoreId == storeId && !si.Sale.IsDeleted && si.Sale.SaleDate >= weekStart)
            .GroupBy(si => si.Product.Name)
            .Select(g => new { ProductName = g.Key, TotalQty = g.Sum(x => x.Quantity), TotalRevenue = g.Sum(x => x.Total) })
            .OrderByDescending(x => x.TotalQty)
            .FirstOrDefaultAsync();

        var monthProfit = await _context.SaleItems
            .Where(si => si.Sale.StoreId == storeId && !si.Sale.IsDeleted && si.Sale.SaleDate >= monthStart)
            .SumAsync(si => (decimal?)((si.UnitPrice - si.CostPrice) * si.Quantity)) ?? 0;

        var thisSaleProfit = sale.Items.Sum(i => (i.UnitPrice - i.CostPrice) * i.Quantity);

        var totalTransactionsToday = await _context.Sales
            .CountAsync(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= todayStart && s.SaleDate < todayStart.AddDays(1));

        var totalTransactionsYesterday = await _context.Sales
            .CountAsync(s => s.StoreId == storeId && !s.IsDeleted && s.SaleDate >= yesterdayStart && s.SaleDate < todayStart);

        var topCategoryThisWeek = await _context.SaleItems
            .Where(si => si.Sale.StoreId == storeId && !si.Sale.IsDeleted && si.Sale.SaleDate >= weekStart)
            .GroupBy(si => si.Product.Category.Name)
            .Select(g => new { CategoryName = g.Key, Revenue = g.Sum(x => x.Total) })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefaultAsync();

        var saleItemProductNames = sale.Items.Select(i => i.ProductName).ToList();
        var saleItemWeekData = await _context.SaleItems
            .Where(si => si.Sale.StoreId == storeId && !si.Sale.IsDeleted && si.Sale.SaleDate >= weekStart && si.Sale.SaleDate < sale.SaleDate)
            .GroupBy(si => si.Product.Name)
            .Select(g => new { ProductName = g.Key, TotalQty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductName, x => x.TotalQty);

        var insights = new List<SaleInsight>();

        if (thisSaleProfit > 0)
        {
            var margin = sale.TotalAmount > 0 ? (thisSaleProfit / sale.TotalAmount * 100) : 0;
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-coins",
                Color = "var(--success)",
                Background = "#dcfce7",
                Title = "Profit from this sale",
                Message = $"You made {CurrencyHelper.FormatCurrency(thisSaleProfit)} profit ({margin:F0}% margin) on this transaction."
            });
        }

        if (yesterdaySalesTotal > 0)
        {
            var change = ((todaySalesTotal - yesterdaySalesTotal) / yesterdaySalesTotal) * 100;
            var direction = change >= 0 ? "increased" : "dropped";
            var icon = change >= 0 ? "fas fa-arrow-trend-up" : "fas fa-arrow-trend-down";
            var color = change >= 0 ? "var(--success)" : "var(--danger)";
            var bg = change >= 0 ? "#dcfce7" : "#fee2e2";
            insights.Add(new SaleInsight
            {
                Icon = icon,
                Color = color,
                Background = bg,
                Title = "Daily sales trend",
                Message = $"Your sales {direction} by {Math.Abs(change):F0}% compared to yesterday ({CurrencyHelper.FormatCurrency(yesterdaySalesTotal)} yesterday)."
            });
        }
        else if (todaySalesTotal > 0)
        {
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-arrow-trend-up",
                Color = "var(--success)",
                Background = "#dcfce7",
                Title = "First sale today",
                Message = $"You've made {CurrencyHelper.FormatCurrency(todaySalesTotal)} in sales so far today. Keep it up!"
            });
        }

        if (bestSellingThisWeek != null)
        {
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-star",
                Color = "var(--warning)",
                Background = "#fef3c7",
                Title = "Best seller this week",
                Message = $"<strong>{bestSellingThisWeek.ProductName}</strong> is your top product with {bestSellingThisWeek.TotalQty} units sold ({CurrencyHelper.FormatCurrency(bestSellingThisWeek.TotalRevenue)} revenue)."
            });
        }

        if (monthProfit > 0)
        {
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-wallet",
                Color = "var(--info)",
                Background = "#dbeafe",
                Title = "Monthly profit estimate",
                Message = $"Your estimated profit for this month is <strong>{CurrencyHelper.FormatCurrency(monthProfit)}</strong> from {CurrencyHelper.FormatCurrency(monthSalesTotal)} in total sales."
            });
        }

        if (topCategoryThisWeek != null)
        {
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-tags",
                Color = "#7c3aed",
                Background = "#ede9fe",
                Title = "Top category this week",
                Message = $"<strong>{topCategoryThisWeek.CategoryName}</strong> leads with {CurrencyHelper.FormatCurrency(topCategoryThisWeek.Revenue)} in revenue this week."
            });
        }

        if (lastWeekSalesTotal > 0 && thisWeekSalesTotal > 0)
        {
            var weekChange = ((thisWeekSalesTotal - lastWeekSalesTotal) / lastWeekSalesTotal) * 100;
            var direction = weekChange >= 0 ? "up" : "down";
            insights.Add(new SaleInsight
            {
                Icon = "fas fa-chart-line",
                Color = weekChange >= 0 ? "var(--success)" : "var(--danger)",
                Background = weekChange >= 0 ? "#dcfce7" : "#fee2e2",
                Title = "Weekly comparison",
                Message = $"This week's sales are {Math.Abs(weekChange):F0}% {direction} from last week ({CurrencyHelper.FormatCurrency(lastWeekSalesTotal)} last week, {CurrencyHelper.FormatCurrency(thisWeekSalesTotal)} this week)."
            });
        }

        foreach (var item in sale.Items)
        {
            var weekQtyBeforeSale = saleItemWeekData.GetValueOrDefault(item.ProductName, 0);
            if (weekQtyBeforeSale > 0)
            {
                var avgPerDay = (decimal)(weekQtyBeforeSale / Math.Max(1, (sale.SaleDate - weekStart).TotalDays));
                var todaySoFar = item.Quantity;
                if (todaySoFar > avgPerDay * 1.5m)
                {
                    insights.Add(new SaleInsight
                    {
                        Icon = "fas fa-bolt",
                        Color = "var(--warning)",
                        Background = "#fef3c7",
                        Title = $"{item.ProductName} is trending",
                        Message = $"You sold {todaySoFar} units of <strong>{item.ProductName}</strong> today, which is above its daily average of {avgPerDay.ToString("F1")}."
                    });
                }
            }
        }

        sale.Insights = insights;

        return sale;
    }

    public async Task<(List<Sale> Items, int TotalCount)> GetDailySalesSummaryAsync(DateTime date, int storeId, int page = 1, int pageSize = 20)
    {
        var query = _context.Sales
            .Where(s => s.SaleDate.Date == date.Date && !s.IsDeleted && s.StoreId == storeId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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
