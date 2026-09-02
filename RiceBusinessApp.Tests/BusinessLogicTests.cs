using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiceBusinessApp.Application.DTOs.Sales;
using RiceBusinessApp.Application.DTOs.Payments;
using RiceBusinessApp.Application.Services;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Domain.Enums;
using RiceBusinessApp.Infrastructure.Data;
using RiceBusinessApp.Infrastructure.Repositories;
using Xunit;

namespace RiceBusinessApp.Tests
{
    public class BusinessLogicTests
    {
        private ServiceProvider GetServiceProvider(string dbName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Register repositories
            // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICustomerTransactionRepository, CustomerTransactionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IPaymentService, PaymentService>();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task SaleService_CreateSale_UpdatesBalancesAndStock()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var serviceProvider = GetServiceProvider(dbName);
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            var product = new Product { BrandName = "E2E Ponni", ProductName = "Rice", BagSize = 25, SellingPrice = 1200, MinimumStockLevel = 10, CurrentStock = 100, IsActive = true, CreatedDate = DateTime.UtcNow };
            var customer = new Customer { Name = "Test Customer", MobileNumber = "1234567890", Address = "Test", IsActive = true, OpeningBalance = 2000, CurrentBalance = 2000, CreatedDate = DateTime.UtcNow };
            
            context.Products.Add(product);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                CustomerId = customer.Id,
                SaleDate = DateTime.UtcNow,
                PaidAmount = 5000,
                PaymentMode = "Cash",
                SaleItems = new List<SaleItemCreateDto>
                {
                    new SaleItemCreateDto { ProductId = product.Id, Quantity = 10, Rate = 1200 }
                }
            };

            // Act
            var result = await saleService.CreateSaleAsync(saleDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12000, result.TotalAmount); // 10 * 1200
            Assert.Equal(7000, result.BalanceAmount); // 12000 - 5000
            
            var dbCustomer = await context.Customers.FindAsync(customer.Id);
            Assert.Equal(9000, dbCustomer.CurrentBalance); // 2000 + 7000

            var dbProduct = await context.Products.FindAsync(product.Id);
            Assert.Equal(90, dbProduct.CurrentStock); // 100 - 10

            var stockTx = await context.StockTransactions.FirstOrDefaultAsync();
            Assert.NotNull(stockTx);
            Assert.Equal(StockTransactionType.Out, stockTx.TransactionType);
            Assert.Equal(10, stockTx.Quantity);
            
            var customerTxCount = await context.CustomerTransactions.CountAsync(t => t.CustomerId == customer.Id);
            Assert.Equal(1, customerTxCount); // One transaction for the net balance debt
        }

        [Fact]
        public async Task PaymentService_CreatePayment_UpdatesCustomerBalance()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var serviceProvider = GetServiceProvider(dbName);
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            var customer = new Customer { Name = "Test Customer", IsActive = true, OpeningBalance = 2000, CurrentBalance = 9000, CreatedDate = DateTime.UtcNow };
            
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var paymentDto = new PaymentCreateDto
            {
                CustomerId = customer.Id,
                Amount = 3000,
                PaymentMode = "Cash",
                PaymentDate = DateTime.UtcNow
            };

            // Act
            var result = await paymentService.CreatePaymentAsync(paymentDto);

            // Assert
            Assert.NotNull(result);
            
            var dbCustomer = await context.Customers.FindAsync(1);
            Assert.Equal(6000, dbCustomer.CurrentBalance); // 9000 - 3000

            var customerTx = await context.CustomerTransactions.FirstOrDefaultAsync();
            Assert.NotNull(customerTx);
            Assert.Equal(CustomerTransactionType.Payment, customerTx.TransactionType);
            Assert.Equal(3000, customerTx.Amount);
        }
    }
}
