using AutoMapper;
using SteamApp.Exceptions;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.Dto;

namespace SteamApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _steamRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository steamRepository, IMapper mapper)
        {
            _steamRepository = steamRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductAsync(long? id)
        {
            if (id == null || id <= 0 || id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {id} doesn't exist.");
            }

            var product = await _steamRepository.GetProductAsync(id.Value);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            var products = await _steamRepository.GetProductsAsync();
            var result = new List<ProductDto>();

            if (products.Any())
            {
                foreach (var product in products)
                {
                    result.Add(_mapper.Map<ProductDto>(product));
                }
            }

            return result;
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto? productDto)
        {
            if (productDto == null)
            {
                throw new NullReferenceException($"Product cannot be null.");
            }

            if (productDto.QualityId == 0)
            {
                productDto.QualityId = null;
            }

            var product = await _steamRepository.CreateProductAsync(productDto);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> CreateProductsAsync(ProductDto[] productDtos)
        {
            if (!productDtos.Any())
            {
                throw new Exception("Empty collection.");
            }

            if (productDtos.Where(x => x.QualityId == 0).Any())
            {
                foreach (var productDto in productDtos.Where(x => x.QualityId == 0))
                {
                    productDto.QualityId = null;
                }
            }

            var products = await _steamRepository.CreateProductsAsync(productDtos);

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<bool> UpdateProductAsync(ProductDto? product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "ProductDto cannot be null.");
            }

            if (product?.Id == null || product.Id <= 0 || product.Id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {product!.Id} doesn't exist.");
            }

            return await _steamRepository.UpdateProductAsync(product);
        }

        public async Task<bool[]> UpdateProductsAsync(ProductDto[] productDtos)
        {
            if (productDtos == null || productDtos.Length == 0)
            {
                throw new ArgumentNullException(nameof(productDtos), "ProductDtos cannot be null or empty.");
            }

            return await _steamRepository.UpdateProductsAsync(productDtos);
        }

        public async Task<bool> DeleteProductAsync(long? id)
        {
            if (id == null || id <= 0 || id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {id} doesn't exist.");
            }

            return await _steamRepository.DeleteProductAsync(id.Value);
        }

        public async Task<bool[]> DeleteProductsAsync(long[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentNullException(nameof(ids), "IDs cannot be null or empty.");
            }

            return await _steamRepository.DeleteProductsAsync(ids);
        }
    }
}
