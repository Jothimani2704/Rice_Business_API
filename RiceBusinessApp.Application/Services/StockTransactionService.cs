using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.StockTransaction;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.Services
{
    public class StockTransactionService : IStockTransactionService
    {
        private readonly IStockTransactionRepository _stockTransactionRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public StockTransactionService(
            IStockTransactionRepository stockTransactionRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork)
        {
            _stockTransactionRepo = stockTransactionRepo;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<StockTransactionDto>> GetAllAsync()
        {
            var transactions = await _stockTransactionRepo.GetAllAsync();
            return transactions.OrderByDescending(t => t.Id).Select(MapToDto);
        }

        public async Task<StockTransactionDto> GetByIdAsync(int id)
        {
            var st = await _stockTransactionRepo.GetByIdAsync(id);
            if (st == null)
                throw new KeyNotFoundException("Stock transaction not found");

            return MapToDto(st);
        }

        public async Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto createDto)
        {
            var product = await _productRepo.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            decimal previousStock = product.CurrentStock;
            decimal change = createDto.TransactionType == StockTransactionType.In ? createDto.Quantity : -createDto.Quantity;
            decimal newStock = previousStock + change;

            var transaction = new StockTransaction
            {
                ProductId = createDto.ProductId,
                TransactionType = createDto.TransactionType,
                Quantity = createDto.Quantity,
                PreviousStock = previousStock,
                NewStock = newStock,
                ReferenceType = createDto.TransactionType == StockTransactionType.In ? "MANUAL_IN" : "MANUAL_OUT",
                TransactionDate = createDto.TransactionDate,
                Notes = createDto.Notes,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                product.CurrentStock = newStock;
                product.UpdatedDate = DateTime.UtcNow;
                
                await _productRepo.UpdateAsync(product);
                await _stockTransactionRepo.AddAsync(transaction);
                
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            var savedTransaction = await _stockTransactionRepo.GetByIdAsync(transaction.Id);
            return MapToDto(savedTransaction!);
        }

        public async Task<StockTransactionDto> UpdateAsync(int id, UpdateStockTransactionDto updateDto)
        {
            var transaction = await _stockTransactionRepo.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException("Stock transaction not found");

            var product = await _productRepo.GetByIdAsync(transaction.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Revert old quantity
                decimal oldChange = transaction.TransactionType == StockTransactionType.In ? transaction.Quantity : -transaction.Quantity;
                product.CurrentStock -= oldChange;

                // Apply new quantity
                decimal newChange = transaction.TransactionType == StockTransactionType.In ? updateDto.Quantity : -updateDto.Quantity;
                product.CurrentStock += newChange;
                product.UpdatedDate = DateTime.UtcNow;
                
                await _productRepo.UpdateAsync(product);

                // Update transaction record
                transaction.PreviousStock = product.CurrentStock - newChange; 
                transaction.Quantity = updateDto.Quantity;
                transaction.NewStock = product.CurrentStock;
                transaction.TransactionDate = updateDto.TransactionDate;
                transaction.Notes = updateDto.Notes;

                await _stockTransactionRepo.UpdateAsync(transaction);
                
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            var updatedTransaction = await _stockTransactionRepo.GetByIdAsync(transaction.Id);
            return MapToDto(updatedTransaction!);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetByProductIdAsync(int productId)
        {
            var transactions = await _stockTransactionRepo.GetHistoryByProductIdAsync(productId);
            return transactions.Select(MapToDto);
        }

        public async Task<StockSummaryDto> GetStockSummaryByProductIdAsync(int productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            var transactions = await _stockTransactionRepo.GetHistoryByProductIdAsync(productId);
            
            decimal totalIn = transactions.Where(t => t.TransactionType == StockTransactionType.In).Sum(t => t.Quantity);
            decimal totalOut = transactions.Where(t => t.TransactionType == StockTransactionType.Out || t.TransactionType == StockTransactionType.Adjustment).Sum(t => t.Quantity);

            return new StockSummaryDto
            {
                ProductId = productId,
                TotalInward = totalIn,
                TotalOutward = totalOut,
                CurrentStock = product.CurrentStock,
                StockValue = product.CurrentStock * product.PurchasePrice
            };
        }

        private static StockTransactionDto MapToDto(StockTransaction st)
        {
            return new StockTransactionDto
            {
                Id = st.Id,
                ProductId = st.ProductId,
                ProductName = st.Product?.ProductName,
                TransactionType = st.TransactionType,
                Quantity = st.Quantity,
                PreviousStock = st.PreviousStock,
                NewStock = st.NewStock,
                CustomerId = st.CustomerId,
                CustomerName = st.Customer?.Name,
                ReferenceType = st.ReferenceType,
                ReferenceId = st.ReferenceId,
                TransactionDate = st.TransactionDate,
                Notes = st.Notes,
                CreatedDate = st.CreatedDate
            };
        }
    }
}
