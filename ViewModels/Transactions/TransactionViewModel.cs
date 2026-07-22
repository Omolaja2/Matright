using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.Transactions;

public class TransactionViewModel
{
    public List<TransactionListItem> Transactions { get; set; } = new();
    public decimal TotalCredit => Transactions.Where(t => t.Direction == TransactionDirection.Credit).Sum(t => t.Amount);
    public decimal TotalDebit => Transactions.Where(t => t.Direction == TransactionDirection.Debit).Sum(t => t.Amount);
    public decimal CurrentBalance => TotalCredit - TotalDebit;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
}

public class TransactionListItem
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionDirection Direction { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal RunningBalance { get; set; }
}
