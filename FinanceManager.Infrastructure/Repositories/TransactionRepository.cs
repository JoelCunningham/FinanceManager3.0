namespace FinanceManager.Infrastructure.Repositories;

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

        return transaction ?? throw new KeyNotFoundException($"Transaction with ID {id} not found.");
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

    public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();
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

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(FilterQuery query)
    {
        var queryable = dbContext.Transactions
            .Include(t => t.Record)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.Category)
            .ThenInclude(c => c!.Group)
            .Include(t => t.Reimbursements)
            .AsQueryable();

        if (query.FilterCategory != null)
        {
            queryable = queryable.Where(t => t.CategoryId == query.FilterCategory.Id);
        }

        if (query.FilterDateFrom != null)
        {
            queryable = queryable.Where(t => t.Date >= query.FilterDateFrom.Value);
        }

        if (query.FilterDateTo != null)
        {
            queryable = queryable.Where(t => t.Date <= query.FilterDateTo.Value);
        }

        queryable = queryable.Where(t => t.ReimbursesId == null);
        return await queryable.ToListAsync();
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

    public Task CreateAsync(IEnumerable<Transaction> transactions)
    {
        return dbContext.BulkInsertOwnedAsync(transactions);
    }

    public async Task CreateOrUpdateAsync(Transaction transaction)
    {
        var existing = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);

        if (existing is null)
        {
            var record = await dbContext.BankRecords.FindAsync(transaction.RecordId)
                ?? throw new KeyNotFoundException($"BankRecord with ID {transaction.RecordId} not found.");

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
            return;
        }

        await ApplyUpdatesAsync(existing, transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        var existing = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id)
            ?? throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");

        await ApplyUpdatesAsync(existing, transaction);
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaction with ID {id} not found.");
        dbContext.Transactions.Remove(transaction);
    }

    public async Task DeleteOrSkipAsync(Guid id)
    {
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction != null)
        {
            dbContext.Transactions.Remove(transaction);
        }
    }

    public async Task<bool> HasTransactionsForCategoryGroupAsync(Guid categoryGroupId)
    {
        return await dbContext.Transactions
            .Include(t => t.Category)
            .AnyAsync(t => t.Category != null && t.Category.GroupId == categoryGroupId);
    }

    public async Task<(DateOnly Min, DateOnly Max)> GetRangeAsync(Guid? categoryGroupId = null)
    {
        var query = dbContext.Transactions.AsQueryable();

        if (categoryGroupId.HasValue)
        {
            query = query
                .Include(t => t.Category)
                .Where(t => t.Category != null && t.Category.GroupId == categoryGroupId.Value);
        }

        var minDate = await query.MinAsync(t => t.Date);
        var maxDate = await query.MaxAsync(t => t.Date);

        return (DateOnly.FromDateTime(minDate), DateOnly.FromDateTime(maxDate));
    }

    private static async Task<PagedResult<Transaction>> GetPagedAsync(FilterQuery query, IQueryable<Transaction> transactions)
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
            var search = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(t.Description, search) ||
                EF.Functions.Like(t.Record.BankAccount.Bank, search) ||
                (t.Category != null && EF.Functions.Like(t.Category.Name, search)) ||
                (t.Category != null && EF.Functions.Like(t.Category.Group.Name, search)) ||
                (t.Record.BankAccount.AccountNumber != null && EF.Functions.Like(t.Record.BankAccount.AccountNumber, search))
            );
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

    private static Task ApplyUpdatesAsync(Transaction existing, Transaction source)
    {
        existing.Description = source.Description;
        existing.Amount = source.Amount;
        existing.Date = source.Date;
        existing.RecordId = source.RecordId;
        existing.CategoryId = source.CategoryId;
        existing.ReimbursesId = source.ReimbursesId;
        return Task.CompletedTask;
    }
}
