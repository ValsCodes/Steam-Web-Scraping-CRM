using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var result = await context.WatchItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (result == null)
        {
            throw new ItemNotFoundException($"Product with ID {id} not found.");
        }

        return result;
    }

    public async Task<IEnumerable<Product>> GetListAsync(CancellationToken ct = default)
    {
        var result = await context.WatchItems.ToListAsync();
        return result;
    }

    public async Task<long> CreateAsync(Product product, CancellationToken ct = default)
    {
        await context.AddAsync(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    public async Task<IEnumerable<long>> CreateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        await context.AddRangeAsync(products);
        await context.SaveChangesAsync();

        return products.Select(x => x.Id);
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
    {
        context.WatchItems.Update(product);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<bool>> UpdateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("UpdateRangeAsync is not implemented yet.");
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var existingListing = await context.WatchItems.FindAsync(id);

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
        var productsToDelete = await context.WatchItems.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

        var foundIds = productsToDelete.Select(p => p.Id).ToHashSet();

        if (productsToDelete.Any())
        {
            context.WatchItems.RemoveRange(productsToDelete);
            await context.SaveChangesAsync(ct);
        }

        return ids.Select(id => foundIds.Contains(id));
    }
}
