namespace PharMarket.ViewModels.Products;

public class ProductListViewModel
{
    public List<ProductListItem> Products { get; set; } = new();
    public string? SearchQuery { get; set; }
    public int? CategoryFilter { get; set; }
    public int? SupplierFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 20;
}

public class ProductListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalesPrice { get; set; }
    public int TotalStock { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string StockStatus => TotalStock <= 0 ? "OutOfStock" : TotalStock <= 10 ? "LowStock" : "InStock";
}

public enum StockStatusLevel
{
    InStock,
    LowStock,
    OutOfStock
}
