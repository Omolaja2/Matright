using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Store;

namespace PharMarket.Services;

public interface IStoreService
{
    Task<Store?> GetStoreByIdAsync(int id);
    Task<List<Store>> GetAllStoresAsync();
    Task<Store> CreateStoreAsync(StoreViewModel model, int ownerId);
    Task UpdateStoreAsync(StoreViewModel model);
    Task<bool> StoreExistsAsync();
    Task<int?> GetFirstStoreIdAsync();
}

public class StoreService : IStoreService
{
    private readonly AppDbContext _context;

    public StoreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Store?> GetStoreByIdAsync(int id)
    {
        return await _context.Stores.FindAsync(id);
    }

    public async Task<List<Store>> GetAllStoresAsync()
    {
        return await _context.Stores
            .Where(s => !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Store> CreateStoreAsync(StoreViewModel model, int ownerId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var store = new Store
            {
                Name = model.Name,
                Address = model.Address,
                Description = model.Description,
                OwnerName = model.OwnerName,
                OwnerPosition = model.OwnerPosition,
                Phone = model.Phone,
                Email = model.Email,
                IsActive = true
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            var owner = await _context.Users.FindAsync(ownerId);
            if (owner != null)
            {
                owner.StoreId = store.Id;
                owner.Role = "Admin";
                owner.Position = model.OwnerPosition ?? "Owner";
            }

            await _context.SaveChangesAsync();

            // Create default categories for the store
            _context.Categories.AddRange(
                new Category { Name = "General", Description = "General products", StoreId = store.Id },
                new Category { Name = "Electronics", Description = "Electronic devices", StoreId = store.Id },
                new Category { Name = "Groceries", Description = "Food and grocery items", StoreId = store.Id },
                new Category { Name = "Household", Description = "Household essentials", StoreId = store.Id },
                new Category { Name = "Personal Care", Description = "Personal hygiene products", StoreId = store.Id }
            );

            // Create default tax setting
            _context.TaxSettings.Add(new TaxSetting
            {
                TaxName = "VAT",
                TaxRate = 7.5m,
                IsEnabled = true,
                StoreId = store.Id
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return store;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateStoreAsync(StoreViewModel model)
    {
        var store = await _context.Stores.FindAsync(model.Id)
            ?? throw new NotFoundException("Store", model.Id);

        store.Name = model.Name;
        store.Address = model.Address;
        store.Description = model.Description;
        store.OwnerName = model.OwnerName;
        store.OwnerPosition = model.OwnerPosition;
        store.Phone = model.Phone;
        store.Email = model.Email;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> StoreExistsAsync()
    {
        return await _context.Stores.AnyAsync(s => !s.IsDeleted);
    }

    public async Task<int?> GetFirstStoreIdAsync()
    {
        var store = await _context.Stores.FirstOrDefaultAsync(s => !s.IsDeleted && s.IsActive);
        return store?.Id;
    }
}
