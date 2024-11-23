using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Models;
using SteamAppServer.Repositories.Interfaces;

namespace SteamAppServer.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Get Product
        public Task<Product?> GetProductAsync(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var result = await _context.Products.ToListAsync();
            return result;
        }
        #endregion

        #region Create Product
        public async Task<Product?> CreateProductAsync(Product product)
        {
            await _context.AddAsync(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<IEnumerable<Product>> CreateProductsAsync(Product[] products)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Delete Product
        public async Task<long?> DeleteProductAsync(long id)
        {
            var existingListing = await _context.Products.FindAsync(id);

            if (existingListing == null)
            {
                return null;
            }

            _context.Remove(existingListing);
            await _context.SaveChangesAsync();

            return existingListing.Id;
        }

        public async Task<IEnumerable<long?>> DeleteProductsAsync(long[] id)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Update Product
        public async Task<Product?> UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.QualityId = product.QualityId;
            existingProduct.Description = product.Description;
            existingProduct.DateBought = product.DateBought;
            existingProduct.DateSold = product.DateSold;
            existingProduct.CostPrice = product.CostPrice;
            existingProduct.TargetSellPrice1 = product.TargetSellPrice1;
            existingProduct.TargetSellPrice2 = product.TargetSellPrice2;
            existingProduct.TargetSellPrice3 = product.TargetSellPrice3;
            existingProduct.TargetSellPrice4 = product.TargetSellPrice4;
            existingProduct.SoldPrice = product.SoldPrice;
            existingProduct.IsHat = product.IsHat;
            existingProduct.IsWeapon = product.IsWeapon;
            existingProduct.IsSold = product.IsSold;

            await _context.SaveChangesAsync();

            return existingProduct;
        }

        public async Task<IEnumerable<Product>> UpdateProductsAsync(Product[] products)
        {
            var existingProducts = await _context.Products.ToListAsync();
            var productsList = new List<Product>();

            foreach (var product in products)
            {
                _context.Update(product);
                if (existingProducts.Any(x => x.Id == product.Id))
                {

                    productsList.Add(product);
                }
                else
                {
                    continue;
                }
            }

            return null;
        }
        #endregion
    }
}
