namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed record SaveReviewResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveReview(ITransactionRepository transactionRepository, ITransferRepository transferRepository, IMachineLearningRepository machineLearningRepository, ILogger<SaveReview> logger)
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
                await ConvertToTransferAsync(group.Transactions.First().EntityId, group.Transfers.EntityId);
            }
        }
        catch
        {
            logger.LogError("An unexpected error occurred while saving review for bank record with ID {BankRecordId}.", group.Record.BankRecordId);
            return new SaveReviewResult([new UseCaseUnexpectedError()]);
        }
        return new SaveReviewResult([]);
    }

    private async Task SaveTransactionsAsync(ReviewGroup group)
    {
        try
        {
            var transactionList = group.Transactions.ToList();
            var transactionIds = transactionList.Select(t => t.EntityId).ToHashSet();
            var entities = transactionList.ToDictionary(t => t.EntityId, t => t.ToTransaction(group.Record));
            var externalEntities = new Dictionary<Guid, Transaction>();

            foreach (var transaction in transactionList)
            {
                if (transaction.Amount == 0) throw new Exception("Transaction must have a nonzero amount.");
                if (transaction.Category is null && transaction.Reimburses is null) throw new Exception("Transaction must have a category.");
                if (transaction.Record.BankRecordId != group.Record.BankRecordId) throw new Exception("Split transactions must use the same bank record.");
                if (transaction.Reimburses is not null && transaction.Reimburses.EntityId == transaction.EntityId) throw new Exception("A transaction cannot reimburse itself.");
                if (transaction.Reimburses is not null && transaction.Reimbursements.Any()) throw new Exception("A transaction that reimburses another cannot be reimbursed.");
            }

            var existingSiblings = await transactionRepository.GetByRecordIdAsync(group.Record.BankRecordId);
            var removedSiblings = existingSiblings.Where(t => !transactionIds.Contains(t.Id)).ToList();

            foreach (var removed in removedSiblings)
            {
                var reimbursementsToClear = await transactionRepository.GetReimbursementsAsync([removed.Id]);
                foreach (var reimbursement in reimbursementsToClear)
                {
                    reimbursement.ReimbursesId = null;
                    externalEntities[reimbursement.Id] = reimbursement;
                }

                await transactionRepository.DeleteOrSkipAsync(removed.Id);
            }

            var desiredReimburses = new Dictionary<Guid, Guid?>();
            var desiredReimbursements = new Dictionary<Guid, HashSet<Guid>>();

            foreach (var reviewTransaction in transactionList)
            {
                if (reviewTransaction.Reimburses is not null)
                {
                    desiredReimburses[reviewTransaction.EntityId] = reviewTransaction.Reimburses.EntityId;
                    continue;
                }

                foreach (var reimbursement in reviewTransaction.Reimbursements)
                {
                    desiredReimburses[reimbursement.EntityId] = reviewTransaction.EntityId;

                    if (!desiredReimbursements.TryGetValue(reviewTransaction.EntityId, out var set))
                    {
                        set = [];
                        desiredReimbursements[reviewTransaction.EntityId] = set;
                    }

                    set.Add(reimbursement.EntityId);
                }
            }

            var reimbursementsById = await transactionRepository.GetReimbursementsAsync(transactionIds.Concat(removedSiblings.Select(t => t.Id)));

            foreach (var reimbursement in reimbursementsById)
            {
                if (reimbursement.ReimbursesId is null) continue;

                if (!desiredReimbursements.TryGetValue(reimbursement.ReimbursesId.Value, out var desired) ||
                    !desired.Contains(reimbursement.Id))
                {
                    reimbursement.ReimbursesId = null;
                    externalEntities[reimbursement.Id] = reimbursement;
                }
            }

            foreach (var reviewTransaction in transactionList)
            {
                var transactionEntity = entities[reviewTransaction.EntityId];
                transactionEntity.ReimbursesId = desiredReimburses.GetValueOrDefault(reviewTransaction.EntityId);
                transactionEntity.Reimburses = null;
            }

            foreach (var (transactionId, reimbursesId) in desiredReimburses)
            {
                if (entities.ContainsKey(transactionId)) continue;

                var externalEntity = await ResolveTransactionAsync(transactionId, entities, externalEntities);
                externalEntity.ReimbursesId = reimbursesId;
                externalEntity.Reimburses = null;
                externalEntities[transactionId] = externalEntity;
            }

            foreach (var transaction in transactionList)
            {
                var categoryId = transaction.Category?.Id;
                var transactionEntity = entities[transaction.EntityId];

                await transactionRepository.CreateOrUpdateAsync(transactionEntity);
                if (categoryId is not null)
                {
                    await machineLearningRepository.CreateAsync(categoryId.Value, transaction.Record.Description);
                }
            }

            foreach (var externalEntity in externalEntities.Values)
            {
                await transactionRepository.UpdateAsync(externalEntity);
            }
        }
        catch
        {
            throw;
        }
    }

    private async Task<Transaction> ResolveTransactionAsync(Guid id, Dictionary<Guid, Transaction> entities, Dictionary<Guid, Transaction> externalEntities)
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
        }
        catch
        {
            logger.LogError("An unexpected error occurred while converting transactions to transfer.");
            throw;
        }
    }
}
