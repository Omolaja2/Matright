using PharMarket.Models.Entities;
using PharMarket.ViewModels.Expenses;
using PharMarket.ViewModels.Transactions;

namespace PharMarket.Services;

public interface IFinanceService
{
    Task<List<Expense>> GetAllExpensesAsync(int storeId, DateTime? startDate, DateTime? endDate, string? category);
    Task<Expense> CreateExpenseAsync(ExpenseViewModel model, int storeId);
    Task UpdateExpenseAsync(ExpenseViewModel model, int storeId);
    Task DeleteExpenseAsync(int id, int storeId);
    Task<decimal> GetCashAtHandAsync(int storeId);
    Task<TransactionViewModel> GetTransactionsAsync(int storeId, DateTime? startDate, DateTime? endDate);
    Task<decimal> GetTotalCapitalAsync(int storeId);
}
