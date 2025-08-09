using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<WatchItem> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var result = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (result == null)
        {
            throw new ItemNotFoundException($"Product with ID {id} not found.");
        }

        return result;
    }

    public async Task<IEnumerable<WatchItem>> GetListAsync(CancellationToken ct = default)
    {
        var result = await context.Products.ToListAsync();
        return result;
    }

    public async Task<long> CreateAsync(WatchItem product, CancellationToken ct = default)
    {
        await context.AddAsync(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    public async Task<IEnumerable<long>> CreateRangeAsync(IEnumerable<WatchItem> products, CancellationToken ct = default)
    {
        await context.AddRangeAsync(products);
        await context.SaveChangesAsync();

        return products.Select(x => x.Id);
    }

    public async Task<bool> UpdateAsync(WatchItem product, CancellationToken ct = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<bool>> UpdateRangeAsync(IEnumerable<WatchItem> products, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("UpdateRangeAsync is not implemented yet.");
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var existingListing = await context.Products.FindAsync(id);

        if (existingListing == null)
        {
            throw new ItemNotFoundException($"Product with ID {id} not found.");
        }

        context.Remove(existingListing);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<bool>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var productsToDelete = await context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

        var foundIds = productsToDelete.Select(p => p.Id).ToHashSet();

        if (productsToDelete.Any())
        {
            context.Products.RemoveRange(productsToDelete);
            await context.SaveChangesAsync(ct);
        }

        return ids.Select(id => foundIds.Contains(id));
    }
}
