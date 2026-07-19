using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.Models.Enums;
using PharMarket.ViewModels.Expenses;
using PharMarket.ViewModels.Transactions;

namespace PharMarket.Services;

public class FinanceService : IFinanceService
{
    private readonly AppDbContext _context;

    public FinanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetAllExpensesAsync(int storeId, DateTime? startDate, DateTime? endDate, string? category)
    {
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.StoreId == storeId);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(e => e.Category == category);

        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<Expense> CreateExpenseAsync(ExpenseViewModel model, int storeId)
    {
        var expense = new Expense
        {
            Description = model.Description,
            Amount = model.Amount,
            Category = model.Category,
            ExpenseDate = model.ExpenseDate,
            PaymentMethod = model.PaymentMethod,
            Receipt = model.Receipt,
            StoreId = storeId
        };

        _context.Expenses.Add(expense);

        _context.Transactions.Add(new Transaction
        {
            Type = TransactionType.Expense,
            Amount = model.Amount,
            Direction = TransactionDirection.Debit,
            PaymentMethod = model.PaymentMethod,
            Description = $"Expense: {model.Description}",
            TransactionDate = DateTime.UtcNow,
            RunningBalance = await CalculateRunningBalanceAsync(storeId) - model.Amount,
            StoreId = storeId
        });

        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task UpdateExpenseAsync(ExpenseViewModel model, int storeId)
    {
        var expense = await _context.Expenses.FindAsync(model.Id)
            ?? throw new NotFoundException("Expense", model.Id);

        if (expense.StoreId != storeId)
            throw new NotFoundException("Expense", model.Id);

        expense.Description = model.Description;
        expense.Amount = model.Amount;
        expense.Category = model.Category;
        expense.ExpenseDate = model.ExpenseDate;
        expense.PaymentMethod = model.PaymentMethod;
        expense.Receipt = model.Receipt;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpenseAsync(int id, int storeId)
    {
        var expense = await _context.Expenses.FindAsync(id)
            ?? throw new NotFoundException("Expense", id);

        if (expense.StoreId != storeId)
            throw new NotFoundException("Expense", id);

        expense.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetCashAtHandAsync(int storeId)
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.PaymentMethod == PaymentMethod.Cash && t.StoreId == storeId)
            .ToListAsync();

        return transactions
            .Where(t => t.Direction == TransactionDirection.Credit)
            .Sum(t => t.Amount) - transactions
            .Where(t => t.Direction == TransactionDirection.Debit)
            .Sum(t => t.Amount);
    }

    public async Task<TransactionViewModel> GetTransactionsAsync(int storeId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.StoreId == storeId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value.AddDays(1));

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionListItem
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Direction = t.Direction,
                PaymentMethod = t.PaymentMethod,
                Description = t.Description,
                TransactionDate = t.TransactionDate,
                RunningBalance = t.RunningBalance
            })
            .ToListAsync();

        return new TransactionViewModel { Transactions = transactions };
    }

    public async Task<decimal> GetTotalCapitalAsync(int storeId)
    {
        return await _context.Capitals
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.StoreId == storeId)
            .SumAsync(c => c.Type == CapitalType.Withdrawal ? -c.Amount : c.Amount);
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
