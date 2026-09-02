using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Product;
namespace RiceBusinessApp.Application.Interfaces {
public interface IProductService {
Task<ProductDto?> GetProductByIdAsync(int id);
Task<IEnumerable<ProductDto>> GetAllProductsAsync();
Task<IEnumerable<ProductDto>> SearchProductsAsync(string query);
Task<ProductDto> CreateProductAsync(CreateProductDto dto);
Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
Task<bool> ToggleProductStatusAsync(int id, bool isActive);
} }
