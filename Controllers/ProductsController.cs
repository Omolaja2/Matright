using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.Products;

namespace PharMarket.Controllers;

[Authorize]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;
    private readonly IImageService _imageService;
    private readonly IQrCodeService _qrCodeService;
    private readonly AppDbContext _context;

    public ProductsController(IProductService productService, IImageService imageService, IQrCodeService qrCodeService, AppDbContext context)
    {
        _productService = productService;
        _imageService = imageService;
        _qrCodeService = qrCodeService;
        _context = context;
    }

    private async Task<SelectList> GetCategoriesSelectList(int storeId) =>
        new(await _context.Categories.Where(c => c.StoreId == storeId).ToListAsync(), "Id", "Name");

    private async Task<SelectList> GetSuppliersSelectList(int storeId) =>
        new(await _context.Suppliers.Where(s => s.StoreId == storeId).ToListAsync(), "Id", "Name");

    public async Task<IActionResult> Index(string? search, int? categoryId, int? supplierId, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _productService.GetAllProductsAsync(storeId.Value, search, categoryId, supplierId, page);
        ViewBag.Categories = await GetCategoriesSelectList(storeId.Value);
        ViewBag.Suppliers = await GetSuppliersSelectList(storeId.Value);
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var product = await _productService.GetProductByIdAsync(id, storeId.Value);
        if (product == null) throw new NotFoundException("Product", id);
        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = new ProductViewModel
        {
            Categories = await GetCategoriesSelectList(storeId.Value),
            Suppliers = await GetSuppliersSelectList(storeId.Value)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoriesSelectList(storeId.Value);
            model.Suppliers = await GetSuppliersSelectList(storeId.Value);
            return View(model);
        }

        if (model.ImageFile != null)
        {
            model.ImageUrl = await _imageService.UploadImageAsync(model.ImageFile, storeId.Value);
        }

        var product = await _productService.CreateProductAsync(model, storeId.Value);

        var qrBytes = _qrCodeService.GenerateProductQrCode(product.Id, product.Name);
        var qrUrl = _qrCodeService.SaveQrCode(qrBytes, storeId.Value, product.Id);

        model.QrCodeUrl = qrUrl;
        model.Id = product.Id;

        SetSuccessMessage("Product created successfully.");
        return RedirectToAction(nameof(QrCode), new { id = product.Id });
    }

    public async Task<IActionResult> QrCode(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var product = await _productService.GetProductByIdAsync(id, storeId.Value);
        if (product == null) throw new NotFoundException("Product", id);

        var qrPath = $"/uploads/qrcodes/{storeId}/product_{id}.png";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", qrPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var hasQr = System.IO.File.Exists(fullPath);

        ViewBag.QrCodeUrl = hasQr ? qrPath : null;
        ViewBag.ProductName = product.Name;
        ViewBag.ProductId = product.Id;
        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var product = await _productService.GetProductByIdAsync(id, storeId.Value);
        if (product == null) throw new NotFoundException("Product", id);

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode,
            Description = product.Description,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            CostPrice = product.CostPrice,
            SalesPrice = product.SalesPrice,
            TaxRate = product.TaxRate,
            MinimumStock = product.MinimumStock,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            ExpirationDate = product.ExpirationDate,
            Categories = await GetCategoriesSelectList(storeId.Value),
            Suppliers = await GetSuppliersSelectList(storeId.Value)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoriesSelectList(storeId.Value);
            model.Suppliers = await GetSuppliersSelectList(storeId.Value);
            return View(model);
        }

        if (model.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(model.ImageUrl))
                await _imageService.DeleteImageAsync(model.ImageUrl);
            model.ImageUrl = await _imageService.UploadImageAsync(model.ImageFile, storeId.Value);
        }

        await _productService.UpdateProductAsync(model, storeId.Value);
        SetSuccessMessage("Product updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        await _productService.DeleteProductAsync(id, storeId.Value);
        SetSuccessMessage("Product deleted successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(Array.Empty<object>());

        var products = await _productService.SearchProductsAsync(q, storeId.Value);
        return Json(products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Barcode,
            SalesPrice = p.SalesPrice,
            p.CostPrice,
            p.TaxRate,
            p.ImageUrl,
            StockTotal = p.Stock != null ? p.Stock.StoreQuantity + p.Stock.ShelfQuantity : 0,
            ExpirationDate = p.ExpirationDate?.ToString("yyyy-MM-dd")
        }));
    }
}
