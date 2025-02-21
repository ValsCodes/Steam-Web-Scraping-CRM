using AutoMapper;
using SteamApp.Context;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models;
using SteamApp.Models.Dto;
using SteamApp.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace SteamApp.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMapper _mapper;
        //Add logs if you want
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Get Product
        public async Task<Product> GetProductAsync(long id)
        {
            var result = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (result == null)
            {
                throw new ItemNotFoundException($"Product with ID {id} not found.");
            }

            return result;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var result = await _context.Products.ToListAsync();
            return result;
        }
        #endregion

        #region Create Product
        public async Task<Product> CreateProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            await _context.AddAsync(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<IEnumerable<Product>> CreateProductsAsync(ProductDto[] productDtos)
        {
            if (productDtos == null || productDtos.Length == 0)
            {
                throw new ArgumentNullException(nameof(productDtos), "ProductDtos cannot be null or empty.");
            }

            var products = new List<Product>();

            foreach (var productDto in productDtos)
            {
                var product = _mapper.Map<Product>(productDto);
                products.Add(product);
            }

            await _context.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            return products;
        }
        #endregion

        #region Update Product
        public async Task<bool> UpdateProductAsync(ProductDto product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);

            if (existingProduct == null)
            {
                throw new ItemNotFoundException($"Product with ID {product.Id} not found.");
            }

            _mapper.Map(product, existingProduct);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool[]> UpdateProductsAsync(ProductDto[] productDtos)
        {
            var updateStatuses = new bool[productDtos.Length];
            var productsToUpdate = new List<Product>();

            for (int i = 0; i < productDtos.Length; i++)
            {
                var productDto = productDtos[i];
                var existingProduct = await _context.Products.FindAsync(productDtos[i].Id);

                if (existingProduct == null)
                {
                    updateStatuses[i] = false;
                    continue;
                }

                _mapper.Map(productDto, existingProduct);
                productsToUpdate.Add(existingProduct);
                updateStatuses[i] = true;
            }

            if (productsToUpdate.Count > 0)
            {
                _context.UpdateRange(productsToUpdate);

                await _context.SaveChangesAsync();
            }

            return updateStatuses;
        }
        #endregion

        #region Delete Product

        public async Task<bool> DeleteProductAsync(long id)
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

        public async Task<bool[]> DeleteProductsAsync(long[] ids)
        {
            var deleteStatuses = new bool[ids.Length];
            var productsForDeletion = _context.Products.Where(x => ids.Contains(x.Id)).ToList();

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var product = productsForDeletion.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    deleteStatuses[i] = false;
                }
                else
                {
                    _context.Products.Remove(product);
                    deleteStatuses[i] = true;
                }
            }

            if (productsForDeletion.Any())
            {
                await _context.SaveChangesAsync();
            }

            return deleteStatuses;
        }
        #endregion

    }
}
