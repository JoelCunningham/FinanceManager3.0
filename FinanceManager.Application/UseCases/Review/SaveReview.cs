namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Entities;

public sealed record SaveReviewResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveReview(ITransactionRepository transactionRepository, ITransferRepository transferRepository, IMachineLearningRepository machineLearningRepository, IUnitOfWork unitOfWork)
{
    public async Task<SaveReviewResult> ExecuteAsync(ReviewGroup group)
    {
        if (group.Transactions.Count == 0)
        {
            return new SaveReviewResult([new UseCaseInvalidOperationError("Transaction group must contain at least one transaction.")]);
        }
        if (group.Transfers is not null && group.Transactions.Count > 1)
        {
            return new SaveReviewResult([new UseCaseInvalidOperationError("Transfer transactions cannot be split.")]);
        }
        try
        {
            if (group.Transfers is null)
            {
                await SaveTransactionsAsync(group);
            }
            else
            {
                await ConvertToTransferAsync(group.InitialTransaction.Id, group.Transfers.EntityId);
            }
        }
        catch
        {
            // TODO: Log exception
            return new SaveReviewResult([new UseCaseUnexpectedError()]);
        }
        return new SaveReviewResult([]);
    }

    private async Task SaveTransactionsAsync(ReviewGroup group)
    {
        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionList = group.Transactions.ToList();
            var entities = transactionList.ToDictionary(t => t.EntityId, CreateBaseEntity);
            var externalEntities = new Dictionary<Guid, Transaction>();
            var retainedIds = transactionList.Select(t => t.EntityId).ToHashSet();

            foreach (var transaction in transactionList)
            {
                if (transaction.Amount == 0) throw new Exception("Transaction must have a nonzero amount.");
                if (transaction.Category is null && transaction.Reimburses is null) throw new Exception("Transaction must have a category.");
            }

            var recordTransactions = group.InitialTransaction.Record.Transactions ?? [];
            var removedSplits = recordTransactions.Where(t => !retainedIds.Contains(t.Id)).ToList();
            foreach (var removed in removedSplits)
            {
                if (removed.ReimbursesId is not null)
                {
                    var previousReimburses = await ResolveTransactionAsync(removed.ReimbursesId.Value, entities, externalEntities);
                    previousReimburses.Reimbursements = previousReimburses.Reimbursements.Where(r => r.Id != removed.Id).ToList();
                }

                foreach (var reimbursement in removed.Reimbursements)
                {
                    var reimbursementEntity = await ResolveTransactionAsync(reimbursement.Id, entities, externalEntities);
                    reimbursementEntity.ReimbursesId = null;
                    reimbursementEntity.Reimburses = null;
                }

                recordTransactions.Remove(removed);
                await transactionRepository.DeleteOrSkipAsync(removed.Id);
            }

            foreach (var entity in entities.Values)
            {
                entity.Siblings = entities.Values.Where(e => e.Id != entity.Id).ToList();
            }

            foreach (var reviewTransaction in transactionList)
            {
                var transactionEntity = entities[reviewTransaction.EntityId];
                var existingTransaction = await transactionRepository.GetOrDefaultAsync(reviewTransaction.EntityId);

                var desiredReimbursements = reviewTransaction.Reimbursements.Select(r => r.EntityId).ToHashSet();
                transactionEntity.Reimbursements.Clear();

                if (existingTransaction?.Reimbursements is not null)
                {
                    foreach (var removed in existingTransaction.Reimbursements.Where(r => !desiredReimbursements.Contains(r.Id)))
                    {
                        var removedEntity = await ResolveTransactionAsync(removed.Id, entities, externalEntities);
                        removedEntity.ReimbursesId = null;
                        removedEntity.Reimburses = null;
                    }
                }

                foreach (var reimbursement in reviewTransaction.Reimbursements)
                {
                    var reimbursementEntity = await ResolveTransactionAsync(reimbursement.EntityId, entities, externalEntities);
                    reimbursementEntity.ReimbursesId = transactionEntity.Id;
                    reimbursementEntity.Reimburses = transactionEntity;
                    if (transactionEntity.Reimbursements.All(r => r.Id != reimbursementEntity.Id))
                    {
                        transactionEntity.Reimbursements.Add(reimbursementEntity);
                    }
                }

                if (reviewTransaction.Reimburses is not null)
                {
                    var reimbursesEntity = await ResolveTransactionAsync(reviewTransaction.Reimburses.EntityId, entities, externalEntities);
                    transactionEntity.ReimbursesId = reimbursesEntity.Id;
                    transactionEntity.Reimburses = reimbursesEntity;

                    reimbursesEntity.Reimbursements ??= new List<Transaction>();
                    if (reimbursesEntity.Reimbursements.All(r => r.Id != transactionEntity.Id))
                    {
                        reimbursesEntity.Reimbursements.Add(transactionEntity);
                    }
                }
                else if (existingTransaction?.ReimbursesId is not null)
                {
                    var previousReimburses = await ResolveTransactionAsync(existingTransaction.ReimbursesId.Value, entities, externalEntities);
                    previousReimburses.Reimbursements = previousReimburses.Reimbursements.Where(r => r.Id != transactionEntity.Id).ToList();
                }
            }

            foreach (var transaction in transactionList)
            {
                var categoryEntity = transaction.Category?.ToCategory();
                var transactionEntity = entities[transaction.EntityId];

                await transactionRepository.CreateOrUpdateAsync(transactionEntity);
                if (categoryEntity is not null)
                {
                    await machineLearningRepository.SaveAsync(categoryEntity, transaction.Description);
                }
            }

            foreach (var externalEntity in externalEntities.Values)
            {
                await transactionRepository.CreateOrUpdateAsync(externalEntity);
            }

            await operations.CommitAsync();
        }
        catch
        {
            await operations.RollbackAsync();
            throw;
        }
    }

    private static Transaction CreateBaseEntity(ReviewTransaction transaction)
    {
        return new Transaction
        {
            Id = transaction.EntityId,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date,
            RecordId = transaction.Record.BankRecordId,
            Record = transaction.Record.ToBankRecord(),
            CategoryId = transaction.Category?.Id,
            Category = transaction.Category?.ToCategory(),
            Siblings = [],
            Reimbursements = [],
        };
    }

    private async Task<Transaction> ResolveTransactionAsync(Guid id, IReadOnlyDictionary<Guid, Transaction> entities, IDictionary<Guid, Transaction> externalEntities)
    {
        if (entities.TryGetValue(id, out var entity))
        {
            return entity;
        }

        if (externalEntities.TryGetValue(id, out var externalEntity))
        {
            return externalEntity;
        }

        var loaded = await transactionRepository.GetByIdAsync(id);
        externalEntities[id] = loaded;
        return loaded;
    }

    private async Task ConvertToTransferAsync(Guid transactionIdA, Guid transactionIdB)
    {
        if (transactionIdA == transactionIdB) throw new Exception("Transaction IDs must be different.");

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionA = await transactionRepository.GetByIdAsync(transactionIdA);
            var transactionB = await transactionRepository.GetByIdAsync(transactionIdB);

            if (transactionA.Record.Transactions.Count > 1 || transactionB.Record.Transactions.Count > 1)
            {
                throw new Exception("Split transactions cannot be transfers.");
            }

            await transactionRepository.DeleteAsync(transactionA.Id);
            await transactionRepository.DeleteAsync(transactionB.Id);

            var transfer = EntityConverter.BankRecordPairToTransfer(transactionA.Record, transactionB.Record);

            await transferRepository.CreateAsync(transfer);

            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }
}
