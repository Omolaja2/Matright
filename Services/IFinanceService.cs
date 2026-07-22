using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Expenses;
using PharMarket.ViewModels.Transactions;

namespace PharMarket.Services;

public interface IFinanceService
{
    Task<(List<Expense> Items, int TotalCount)> GetAllExpensesAsync(int storeId, DateTime? startDate, DateTime? endDate, string? category, int page = 1, int pageSize = 20);
    Task<Expense> CreateExpenseAsync(ExpenseViewModel model, int storeId);
    Task UpdateExpenseAsync(ExpenseViewModel model, int storeId);
    Task DeleteExpenseAsync(int id, int storeId);
    Task<decimal> GetCashAtHandAsync(int storeId);
    Task<(TransactionViewModel Model, int TotalCount)> GetTransactionsAsync(int storeId, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 20);
    Task<decimal> GetTotalCapitalAsync(int storeId);
}
