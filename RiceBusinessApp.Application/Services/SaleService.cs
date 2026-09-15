using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Sales;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly ICustomerTransactionRepository _customerTransactionRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SaleService(
            ISaleRepository saleRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IStockTransactionRepository stockTransactionRepository,
            ICustomerTransactionRepository customerTransactionRepository,
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork)
        {
            _saleRepository = saleRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _stockTransactionRepository = stockTransactionRepository;
            _customerTransactionRepository = customerTransactionRepository;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SaleResponseDto> CreateSaleAsync(SaleCreateDto request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Validate Customer
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
                if (customer == null || !customer.IsActive)
                    throw new Exception("Invalid or inactive customer.");

                // 2. Validate Items and Process Stock
                if (request.SaleItems == null || !request.SaleItems.Any())
                    throw new Exception("Sale must contain at least one item.");

                decimal totalAmount = 0;
                var saleItems = new List<SaleItem>();
                var saleDate = request.SaleDate ?? DateTime.UtcNow;

                var stockTransactionsToSave = new List<StockTransaction>();

                foreach (var itemDto in request.SaleItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null || !product.IsActive)
                        throw new Exception($"Invalid or inactive product with ID: {itemDto.ProductId}");

                    if (itemDto.Quantity <= 0)
                        throw new Exception("Quantity must be greater than zero.");
                    if (itemDto.Rate < 0)
                        throw new Exception("Rate cannot be negative.");

                    if (product.CurrentStock < itemDto.Quantity)
                        throw new Exception($"Insufficient stock for product '{product.ProductName}'. Available: {product.CurrentStock}");

                    decimal itemAmount = itemDto.Quantity * itemDto.Rate;
                    totalAmount += itemAmount;

                    var saleItem = new SaleItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        Rate = itemDto.Rate,
                        Amount = itemAmount
                    };
                    saleItems.Add(saleItem);

                    // Reduce Stock
                    decimal previousStock = product.CurrentStock;
                    product.CurrentStock -= itemDto.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // Prepare Stock Transaction
                    var stockTx = new StockTransaction
                    {
                        ProductId = product.Id,
                        CustomerId = customer.Id,
                        TransactionDate = saleDate,
                        TransactionType = StockTransactionType.Out,
                        Quantity = itemDto.Quantity,
                        PreviousStock = previousStock,
                        NewStock = product.CurrentStock,
                        Notes = $"Sale to {customer.Name}",
                        ReferenceType = "SALE"
                    };
                    stockTransactionsToSave.Add(stockTx);
                }

                // 3. Handle Payment and Balance
                if (request.PaidAmount < 0)
                    throw new Exception("Paid Amount cannot be negative.");
                if (request.PaidAmount > totalAmount)
                    throw new Exception("Paid Amount cannot exceed Total Amount.");

                decimal balanceAmount = totalAmount - request.PaidAmount;

                var sale = new Sale
                {
                    CustomerId = customer.Id,
                    SaleDate = saleDate,
                    TotalAmount = totalAmount,
                    PaidAmount = request.PaidAmount,
                    BalanceAmount = balanceAmount,
                    PaymentMode = request.PaymentMode,
                    Notes = request.Notes,
                    SaleItems = saleItems
                };

                // Create Sale (and SaleItems via EF navigation property)
                await _saleRepository.AddAsync(sale);

                // Now sale.Id is populated, assign it to StockTransactions and save them
                foreach (var stockTx in stockTransactionsToSave)
                {
                    stockTx.ReferenceId = sale.Id;
                    await _stockTransactionRepository.AddAsync(stockTx);
                }

                // If PaidAmount > 0, record Payment entry
                if (request.PaidAmount > 0)
                {
                    var paymentRecord = new Payment
                    {
                        CustomerId = customer.Id,
                        Amount = request.PaidAmount,
                        PaymentMode = string.IsNullOrWhiteSpace(request.PaymentMode) ? "Cash" : request.PaymentMode,
                        PaymentDate = saleDate,
                        ReferenceNumber = $"SALE-{sale.Id}",
                        Notes = $"Payment received during Sale #{sale.Id}",
                        PreviousBalance = customer.CurrentBalance,
                        NewBalance = customer.CurrentBalance + balanceAmount,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _paymentRepository.AddAsync(paymentRecord);
                }

                // Update Customer Balance
                if (balanceAmount > 0)
                {
                    customer.CurrentBalance += balanceAmount;
                    await _customerRepository.UpdateAsync(customer);
                    
                    var customerTx = new CustomerTransaction
                    {
                        CustomerId = customer.Id,
                        TransactionDate = saleDate,
                        TransactionType = CustomerTransactionType.Sale,
                        Amount = balanceAmount, // Net debt added
                        ReferenceId = sale.Id,
                        Notes = $"Sale #{sale.Id} (Total: {totalAmount}, Paid: {request.PaidAmount})"
                    };
                    await _customerTransactionRepository.AddAsync(customerTx);
                }
                
                await _unitOfWork.CommitTransactionAsync();

                return await GetSaleByIdAsync(sale.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<SaleResponseDto> UpdateSaleAsync(int id, SaleUpdateDto request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var sale = await _saleRepository.GetByIdAsync(id);
                if (sale == null)
                    throw new Exception("Sale not found.");

                var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);
                if (customer == null || !customer.IsActive)
                    throw new Exception("Invalid or inactive customer.");

                // 1. Revert Old Stock Transactions
                foreach (var oldItem in sale.SaleItems.ToList())
                {
                    var product = await _productRepository.GetByIdAsync(oldItem.ProductId);
                    if (product != null)
                    {
                        decimal previousStock = product.CurrentStock;
                        product.CurrentStock += oldItem.Quantity;
                        await _productRepository.UpdateAsync(product);

                        var stockTx = new StockTransaction
                        {
                            ProductId = product.Id,
                            CustomerId = customer.Id,
                            TransactionDate = DateTime.UtcNow,
                            TransactionType = StockTransactionType.In, // Revert by adding back
                            Quantity = oldItem.Quantity,
                            PreviousStock = previousStock,
                            NewStock = product.CurrentStock,
                            Notes = $"Sale Correction Revert #{sale.Id}",
                            ReferenceType = "SALE_REVERT",
                            ReferenceId = sale.Id
                        };
                        await _stockTransactionRepository.AddAsync(stockTx);
                    }
                }

                // 2. Revert Old Customer Balance
                if (sale.BalanceAmount > 0)
                {
                    customer.CurrentBalance -= sale.BalanceAmount;
                    
                    var customerTx = new CustomerTransaction
                    {
                        CustomerId = customer.Id,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = CustomerTransactionType.Payment, // Revert by simulating payment
                        Amount = sale.BalanceAmount,
                        ReferenceId = sale.Id,
                        Notes = $"Sale Correction Revert #{sale.Id}"
                    };
                    await _customerTransactionRepository.AddAsync(customerTx);
                }

                // Clear old items (EF Core will delete them if configured, but since it's hard we can just remove)
                sale.SaleItems.Clear();

                // 3. Apply New Items and Stock
                if (request.SaleItems == null || !request.SaleItems.Any())
                    throw new Exception("Sale must contain at least one item.");

                decimal newTotalAmount = 0;
                var stockTransactionsToSave = new List<StockTransaction>();

                foreach (var itemDto in request.SaleItems)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null || !product.IsActive)
                        throw new Exception($"Invalid or inactive product with ID: {itemDto.ProductId}");

                    if (itemDto.Quantity <= 0)
                        throw new Exception("Quantity must be greater than zero.");
                    if (itemDto.Rate < 0)
                        throw new Exception("Rate cannot be negative.");

                    if (product.CurrentStock < itemDto.Quantity)
                        throw new Exception($"Insufficient stock for product '{product.ProductName}'. Available: {product.CurrentStock}");

                    decimal itemAmount = itemDto.Quantity * itemDto.Rate;
                    newTotalAmount += itemAmount;

                    var saleItem = new SaleItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        Rate = itemDto.Rate,
                        Amount = itemAmount,
                        SaleId = sale.Id
                    };
                    sale.SaleItems.Add(saleItem);

                    // Reduce Stock
                    decimal previousStock = product.CurrentStock;
                    product.CurrentStock -= itemDto.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // Prepare Stock Transaction
                    var stockTx = new StockTransaction
                    {
                        ProductId = product.Id,
                        CustomerId = customer.Id,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = StockTransactionType.Out,
                        Quantity = itemDto.Quantity,
                        PreviousStock = previousStock,
                        NewStock = product.CurrentStock,
                        Notes = $"Sale Correction Apply #{sale.Id}",
                        ReferenceType = "SALE",
                        ReferenceId = sale.Id
                    };
                    stockTransactionsToSave.Add(stockTx);
                }

                foreach (var stockTx in stockTransactionsToSave)
                {
                    await _stockTransactionRepository.AddAsync(stockTx);
                }

                // 4. Handle Payment and Balance
                if (request.PaidAmount < 0)
                    throw new Exception("Paid Amount cannot be negative.");
                if (request.PaidAmount > newTotalAmount)
                    throw new Exception("Paid Amount cannot exceed Total Amount.");

                decimal oldPaidAmount = sale.PaidAmount;
                decimal newBalanceAmount = newTotalAmount - request.PaidAmount;

                sale.TotalAmount = newTotalAmount;
                sale.PaidAmount = request.PaidAmount;
                sale.BalanceAmount = newBalanceAmount;
                if (request.Notes != null) sale.Notes = request.Notes;

                await _saleRepository.UpdateAsync(sale);

                // If additional payment was made during update, record Payment entry
                decimal additionalPaid = request.PaidAmount - oldPaidAmount;
                if (additionalPaid > 0)
                {
                    var paymentRecord = new Payment
                    {
                        CustomerId = customer.Id,
                        Amount = additionalPaid,
                        PaymentMode = string.IsNullOrWhiteSpace(sale.PaymentMode) ? "Cash" : sale.PaymentMode,
                        PaymentDate = DateTime.UtcNow,
                        ReferenceNumber = $"SALE-{sale.Id}",
                        Notes = $"Payment during Sale update #{sale.Id}",
                        PreviousBalance = customer.CurrentBalance,
                        NewBalance = customer.CurrentBalance + newBalanceAmount,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _paymentRepository.AddAsync(paymentRecord);
                }

                // Update Customer Balance
                if (newBalanceAmount > 0)
                {
                    customer.CurrentBalance += newBalanceAmount;
                    
                    var customerTx = new CustomerTransaction
                    {
                        CustomerId = customer.Id,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = CustomerTransactionType.Sale,
                        Amount = newBalanceAmount, // Net debt added
                        ReferenceId = sale.Id,
                        Notes = $"Sale Correction Apply #{sale.Id} (Total: {newTotalAmount}, Paid: {request.PaidAmount})"
                    };
                    await _customerTransactionRepository.AddAsync(customerTx);
                }
                
                await _customerRepository.UpdateAsync(customer);

                await _unitOfWork.CommitTransactionAsync();

                return await GetSaleByIdAsync(sale.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<SaleResponseDto> GetSaleByIdAsync(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) throw new Exception("Sale not found.");
            return MapToResponseDto(sale);
        }

        public async Task<IEnumerable<SaleListDto>> GetAllSalesAsync()
        {
            var sales = await _saleRepository.GetAllAsync();
            return sales.OrderByDescending(s => s.Id).Select(MapToListDto);
        }

        public async Task<IEnumerable<SaleListDto>> GetSalesByCustomerIdAsync(int customerId)
        {
            var sales = await _saleRepository.GetByCustomerIdAsync(customerId);
            return sales.Select(MapToListDto);
        }

        public async Task<IEnumerable<SaleListDto>> GetSalesByProductIdAsync(int productId)
        {
            var sales = await _saleRepository.GetByProductIdAsync(productId);
            return sales.Select(MapToListDto);
        }

        private SaleResponseDto MapToResponseDto(Sale sale)
        {
            decimal currentBal = sale.Customer?.CurrentBalance ?? 0;
            decimal prevBal = currentBal - sale.BalanceAmount;

            return new SaleResponseDto
            {
                Id = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer?.Name ?? string.Empty,
                CustomerPhone = sale.Customer?.MobileNumber ?? string.Empty,
                CustomerAddress = sale.Customer?.Address ?? string.Empty,
                CustomerCurrentBalance = currentBal,
                PreviousBalance = prevBal,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                BalanceAmount = sale.BalanceAmount,
                PaymentMode = sale.PaymentMode,
                Notes = sale.Notes,
                CreatedDate = sale.CreatedDate,
                SaleItems = sale.SaleItems.Select(si => new SaleItemResponseDto
                {
                    Id = si.Id,
                    ProductId = si.ProductId,
                    ProductName = si.Product?.ProductName ?? string.Empty,
                    BrandName = si.Product?.BrandName ?? string.Empty,
                    BagSize = si.Product?.BagSize ?? 0,
                    Quantity = si.Quantity,
                    Rate = si.Rate,
                    Amount = si.Amount
                }).ToList()
            };
        }

        private SaleListDto MapToListDto(Sale sale)
        {
            return new SaleListDto
            {
                Id = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer?.Name ?? string.Empty,
                CustomerPhone = sale.Customer?.MobileNumber ?? string.Empty,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                BalanceAmount = sale.BalanceAmount,
                PaymentMode = sale.PaymentMode,
                ItemCount = sale.SaleItems?.Count ?? 0,
                TotalQuantity = sale.SaleItems?.Sum(si => si.Quantity) ?? 0
            };
        }
    }
}
