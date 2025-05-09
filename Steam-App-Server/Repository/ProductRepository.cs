using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Context;
using SteamApp.Exceptions;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models;

namespace SteamApp.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Product> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var result = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (result == null)
            {
                throw new ItemNotFoundException($"Product with ID {id} not found.");
            }

            return result;
        }

        public async Task<IEnumerable<Product>> GetListAsync(CancellationToken ct = default)
        {
            var result = await _context.Products.ToListAsync();
            return result;
        }

        public async Task<long> CreateAsync(Product product, CancellationToken ct = default)
        {
            await _context.AddAsync(product);
            await _context.SaveChangesAsync();

            return product.Id;
        }

        public async Task<IEnumerable<long>> CreateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
        {
            await _context.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            return products.Select(x => x.Id);
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("UpdateRangeAsync is not implemented yet.");
        }

        public async Task<IEnumerable<bool>> UpdateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("UpdateRangeAsync is not implemented yet.");
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
        {
            var existingListing = await _context.Products.FindAsync(id);

            if (existingListing == null)
            {
                throw new ItemNotFoundException($"Product with ID {id} not found.");
            }

            _context.Remove(existingListing);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<bool>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default)
        {
            var productsToDelete = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

            var foundIds = productsToDelete.Select(p => p.Id).ToHashSet();

            if (productsToDelete.Any())
            {
                _context.Products.RemoveRange(productsToDelete);
                await _context.SaveChangesAsync(ct);
            }

            return ids.Select(id => foundIds.Contains(id));
        }
    }
}
