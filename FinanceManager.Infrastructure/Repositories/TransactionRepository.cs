namespace FinanceManager.Infrastructure.Repositories.EfCore;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class TransactionRepository(FinanceManagerDbContext dbContext) : ITransactionRepository
{
    public async Task<Transaction> GetByIdAsync(Guid id)
    {
        var transaction = await dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimbursements)
            .Include(t => t.Reimburses)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction with ID {id} not found.");
        }

        return transaction;
    }

    public async Task<Transaction?> GetOrDefaultAsync(Guid id)
    {
        return await dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimbursements)
            .Include(t => t.Reimburses)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByRecordIdAsync(Guid recordId, Guid? excludeId = null)
    {
        var query = dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimburses)
            .Where(t => t.RecordId == recordId);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetReimbursementsAsync(IEnumerable<Guid> reimbursedTransactionIds)
    {
        var ids = reimbursedTransactionIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Where(t => t.ReimbursesId != null && ids.Contains(t.ReimbursesId.Value))
            .ToListAsync();
    }

    public async Task<PagedResult<Transaction>> GetPagedTransactionsAsync(FilterQuery query)
    {
        var queryable = dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimbursements)
            .AsQueryable();

        queryable = queryable.Where(t => t.ReimbursesId == null);
        return await GetPagedAsync(query, queryable);
    }

    public async Task<PagedResult<Transaction>> GetPagedReimbursementsAsync(FilterQuery query)
    {
        var queryable = dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimburses)
            .AsQueryable();

        queryable = queryable.Where(t => t.ReimbursesId != null);
        return await GetPagedAsync(query, queryable);
    }

    public async Task CreateAsync(Transaction transaction)
    {
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
    }

    public async Task CreateAsync(IEnumerable<Transaction> transactions)
    {
        dbContext.Transactions.AddRange(transactions);
        await dbContext.SaveChangesAsync();
    }

    public async Task CreateOrUpdateAsync(Transaction transaction)
    {
        var existing = await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        if (existing is null)
        {
            var record = await dbContext.BankRecords.FindAsync(transaction.RecordId);
            if (record is null)
            {
                throw new KeyNotFoundException($"BankRecord with ID {transaction.RecordId} not found.");
            }

            Category? category = null;
            if (transaction.CategoryId is not null)
            {
                category = await dbContext.Categories.FindAsync(transaction.CategoryId.Value);
            }

            var newTransaction = new Transaction
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                RecordId = transaction.RecordId,
                Record = record,
                CategoryId = transaction.CategoryId,
                Category = category,
                ReimbursesId = transaction.ReimbursesId,
                Reimburses = null,
            };

            dbContext.Transactions.Add(newTransaction);
            await dbContext.SaveChangesAsync();
            return;
        }

        await ApplyUpdatesAsync(existing, transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        var existing = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");
        }

        await ApplyUpdatesAsync(existing, transaction);
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction with ID {id} not found.");
        }

        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteOrSkipAsync(Guid id)
    {
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction != null)
        {
            dbContext.Transactions.Remove(transaction);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<PagedResult<Transaction>> GetPagedAsync(FilterQuery query, IQueryable<Transaction> transactions)
    {
        transactions = ApplyFilters(transactions, query);

        var totalItems = await transactions.CountAsync();
        transactions = ApplySorting(transactions, query);

        var items = await ApplyPagination(transactions, query).ToListAsync();

        return new PagedResult<Transaction>
        {
            Items = items,
            TotalItems = totalItems,
            CurrentPage = query.Page,
            PageSize = query.PageSize
        };
    }

    private static IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query, FilterQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(t =>
                t.Description.Contains(search) ||
                t.Record.BankAccount.Bank.Contains(search) ||
                (t.Record.BankAccount.AccountNumber != null && t.Record.BankAccount.AccountNumber.Contains(search)));
        }

        if (request.FilterCategory != null)
        {
            if (request.FilterCategory.Id != Guid.Empty)
            {
                query = query.Where(t => t.CategoryId == request.FilterCategory.Id);
            }
            else
            {
                query = query.Where(t => t.CategoryId == null);
            }
        }

        if (request.FilterStatus == ReviewStatus.Reviewed)
        {
            query = query.Where(t => t.CategoryId != null);
        }
        if (request.FilterStatus == ReviewStatus.Unreviewed)
        {
            query = query.Where(t => t.CategoryId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.FilterAccountFrom))
        {
            query = query.Where(t => $"{t.Record.BankAccount.Bank} - {t.Record.BankAccount.AccountNumber}" == request.FilterAccountFrom);
        }

        if (request.FilterDateFrom.HasValue)
        {
            query = query.Where(t => t.Date >= request.FilterDateFrom.Value);
        }
        if (request.FilterDateTo.HasValue)
        {
            query = query.Where(t => t.Date <= request.FilterDateTo.Value);
        }

        if (request.FilterAmountMin.HasValue)
        {
            query = query.Where(t => t.Amount + t.Reimbursements.Sum(r => r.Amount) >= request.FilterAmountMin.Value);
        }
        if (request.FilterAmountMax.HasValue)
        {
            query = query.Where(t => t.Amount + t.Reimbursements.Sum(r => r.Amount) <= request.FilterAmountMax.Value);
        }

        return query;
    }

    private static IQueryable<Transaction> ApplySorting(IQueryable<Transaction> query, FilterQuery request)
    {
        return request.SortBy switch
        {
            TransactionSortBy.Category => request.SortDescending
                ? query.OrderByDescending(t => t.Category != null ? t.Category.Name : string.Empty)
                : query.OrderBy(t => t.Category != null ? t.Category.Name : string.Empty),

            TransactionSortBy.Amount => request.SortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),

            TransactionSortBy.Date or _ => request.SortDescending
                ? query.OrderByDescending(t => t.Date)
                : query.OrderBy(t => t.Date)
        };
    }

    private static IQueryable<Transaction> ApplyPagination(IQueryable<Transaction> query, FilterQuery request)
    {
        return query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
    }

    private async Task ApplyUpdatesAsync(Transaction existing, Transaction source)
    {
        existing.Description = source.Description;
        existing.Amount = source.Amount;
        existing.Date = source.Date;
        existing.RecordId = source.RecordId;
        existing.CategoryId = source.CategoryId;
        existing.ReimbursesId = source.ReimbursesId;

        await dbContext.SaveChangesAsync();
    }
}
