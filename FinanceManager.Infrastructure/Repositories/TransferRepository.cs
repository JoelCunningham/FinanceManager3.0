namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class TransferRepository(IFinanceManagerDbContextFactory dbContextFactory) : ITransferRepository
{
    public async Task<IEnumerable<Transfer>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Transfers
            .Include(t => t.FromRecord)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.ToRecord)
            .ThenInclude(r => r.BankAccount)
            .ToListAsync();
    }
    public async Task<Transfer> GetByIdAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var transfer = await dbContext.Transfers
            .Include(t => t.FromRecord)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.ToRecord)
            .ThenInclude(r => r.BankAccount)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transfer with id {id} not found.");

        return transfer;
    }

    public async Task<PagedResult<Transfer>> GetPagedAsync(FilterQuery query)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var queryable = dbContext.Transfers
            .Include(t => t.FromRecord)
            .ThenInclude(r => r.BankAccount)
            .Include(t => t.ToRecord)
            .ThenInclude(r => r.BankAccount)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query);

        var totalItems = await queryable.CountAsync();
        queryable = ApplySorting(queryable, query);

        var items = await ApplyPagination(queryable, query).ToListAsync();

        return new PagedResult<Transfer>
        {
            Items = items,
            TotalItems = totalItems,
            CurrentPage = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task CreateAsync(Transfer transfer)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Transfers.AddAsync(transfer);
        await dbContext.SaveChangesAsync();
    }

    public async Task CreateAsync(IEnumerable<Transfer> transfers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.BulkInsertOwnedAsync(transfers);
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var transfer = await dbContext.Transfers.FirstOrDefaultAsync(t => t.Id == id)
             ?? throw new KeyNotFoundException($"Transfer with id {id} not found.");

        dbContext.Transfers.Remove(transfer);
        await dbContext.SaveChangesAsync();
    }

    private static IQueryable<Transfer> ApplyFilters(IQueryable<Transfer> query, FilterQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(t =>
                t.Description.Contains(search) ||
                t.FromRecord.BankAccount.Bank.Contains(search) ||
                t.ToRecord.BankAccount.Bank.Contains(search) ||
                (t.FromRecord.BankAccount.AccountNumber != null && t.FromRecord.BankAccount.AccountNumber.Contains(search)) ||
                (t.ToRecord.BankAccount.AccountNumber != null && t.ToRecord.BankAccount.AccountNumber.Contains(search)));
        }

        if (request.FilterSource != TransferSource.All)
        {
            var isUserCreated = request.FilterSource == TransferSource.User;
            query = query.Where(t => t.IsUserCreated == isUserCreated);
        }

        if (!string.IsNullOrWhiteSpace(request.FilterAccountFrom))
        {
            query = query.Where(t =>
                $"{t.FromRecord.BankAccount.Bank} - {t.FromRecord.BankAccount.AccountNumber}" == request.FilterAccountFrom);
        }
        if (!string.IsNullOrWhiteSpace(request.FilterAccountTo))
        {
            query = query.Where(t =>
                $"{t.ToRecord.BankAccount.Bank} - {t.ToRecord.BankAccount.AccountNumber}" == request.FilterAccountTo);
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
            query = query.Where(t => t.Amount >= request.FilterAmountMin.Value);
        }
        if (request.FilterAmountMax.HasValue)
        {
            query = query.Where(t => t.Amount <= request.FilterAmountMax.Value);
        }

        return query;
    }

    private static IQueryable<Transfer> ApplySorting(IQueryable<Transfer> query, FilterQuery request)
    {
        return request.SortBy switch
        {
            TransactionSortBy.Amount => request.SortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),

            TransactionSortBy.FromAccount => request.SortDescending
                ? query.OrderByDescending(t => t.FromRecord.BankAccount.Bank)
                    .ThenByDescending(t => t.FromRecord.BankAccount.AccountNumber)
                : query.OrderBy(t => t.FromRecord.BankAccount.Bank)
                    .ThenBy(t => t.FromRecord.BankAccount.AccountNumber),

            TransactionSortBy.ToAccount => request.SortDescending
                ? query.OrderByDescending(t => t.ToRecord.BankAccount.Bank)
                    .ThenByDescending(t => t.ToRecord.BankAccount.AccountNumber)
                : query.OrderBy(t => t.ToRecord.BankAccount.Bank)
                    .ThenBy(t => t.ToRecord.BankAccount.AccountNumber),

            TransactionSortBy.Date or _ => request.SortDescending
                ? query.OrderByDescending(t => t.Date)
                : query.OrderBy(t => t.Date)
        };
    }

    private static IQueryable<Transfer> ApplyPagination(IQueryable<Transfer> query, FilterQuery request)
    {
        return query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
    }
}
