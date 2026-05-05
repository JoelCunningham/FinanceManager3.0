namespace FinanceManager.Infrastructure.Data;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class FinanceManagerDbContext(DbContextOptions<FinanceManagerDbContext> options) : DbContext(options), IDataStore
{
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankRecord> BankRecords => Set<BankRecord>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryGroup> CategoryGroups => Set<CategoryGroup>();
    public DbSet<MachineLearning> MachineLearning => Set<MachineLearning>();
    public DbSet<BudgetYear> BudgetYears => Set<BudgetYear>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();

    public Task SaveAsync() { return SaveChangesAsync(); }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>()
            .HasMany(a => a.BankRecords)
            .WithOne(r => r.BankAccount)
            .HasForeignKey(r => r.BankAccountId);

        modelBuilder.Entity<BankRecord>()
            .HasMany(r => r.Transactions)
            .WithOne(t => t.Record)
            .HasForeignKey(t => t.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Reimburses)
            .WithMany(t => t.Reimbursements)
            .HasForeignKey(t => t.ReimbursesId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CategoryGroup>()
            .HasMany(g => g.Categories)
            .WithOne(c => c.Group)
            .HasForeignKey(c => c.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MachineLearning>()
            .HasOne(m => m.Category)
            .WithMany()
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.FromRecord)
            .WithMany()
            .HasForeignKey(t => t.FromRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.ToRecord)
            .WithMany()
            .HasForeignKey(t => t.ToRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BudgetEntry>()
            .HasOne(b => b.BudgetYear)
            .WithMany()
            .HasForeignKey(b => b.BudgetYearId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BudgetEntry>()
            .HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
