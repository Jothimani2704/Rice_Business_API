using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Product;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;

namespace RiceBusinessApp.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.OrderByDescending(p => p.Id).Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string query)
        {
            var products = await _productRepository.SearchAsync(query);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            if (await _productRepository.ExistsByNameAndBrandAsync(dto.ProductName, dto.BrandName))
            {
                throw new InvalidOperationException("A product with this name and brand already exists.");
            }

            var product = new Product
            {
                BrandName = dto.BrandName,
                ProductName = dto.ProductName,
                BagSize = dto.BagSize,
                PurchasePrice = dto.PurchasePrice,
                SellingPrice = dto.SellingPrice,
                MinimumStockLevel = dto.MinimumStockLevel,
                CurrentStock = 0,
                IsActive = true
            };

            await _productRepository.AddAsync(product);
            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            if ((dto.ProductName != product.ProductName || dto.BrandName != product.BrandName) &&
                await _productRepository.ExistsByNameAndBrandAsync(dto.ProductName, dto.BrandName))
            {
                throw new InvalidOperationException("A product with this name and brand already exists.");
            }

            product.BrandName = dto.BrandName;
            product.ProductName = dto.ProductName;
            product.BagSize = dto.BagSize;
            product.PurchasePrice = dto.PurchasePrice;
            product.SellingPrice = dto.SellingPrice;
            product.MinimumStockLevel = dto.MinimumStockLevel;

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        public async Task<bool> ToggleProductStatusAsync(int id, bool isActive)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            if (!isActive && await _productRepository.IsReferencedAsync(id))
            {
                // We just deactivate instead of deleting, but checking references is good.
                // The requirement says: "Do not permanently delete products that are already referenced".
                // Deactivation is allowed.
            }

            product.IsActive = isActive;
            await _productRepository.UpdateAsync(product);
            return true;
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                BrandName = product.BrandName,
                ProductName = product.ProductName,
                BagSize = product.BagSize,
                PurchasePrice = product.PurchasePrice,
                SellingPrice = product.SellingPrice,
                MinimumStockLevel = product.MinimumStockLevel,
                CurrentStock = product.CurrentStock,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };
        }
    }
}
