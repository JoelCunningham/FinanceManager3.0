namespace FinanceManager.Infrastructure.Data;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class FinanceManagerDbContext(DbContextOptions<FinanceManagerDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataStore
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
    public DbSet<Preference> Preferences => Set<Preference>();

    public Task SaveAsync() { return SaveChangesAsync(); }
    public ITransactionScope BeginTransaction() { return new TransactionScope(Database.BeginTransaction()); }

    private sealed class TransactionScope(IDbContextTransaction transaction) : ITransactionScope
    {
        public Task CommitAsync() { return transaction.CommitAsync(); }
        public Task RollbackAsync() { return transaction.RollbackAsync(); }
        public ValueTask DisposeAsync() { return transaction.DisposeAsync(); }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        builder.Entity<BankAccount>()
            .HasMany(a => a.BankRecords)
            .WithOne(r => r.BankAccount)
            .HasForeignKey(r => r.BankAccountId);

        builder.Entity<BankRecord>()
            .HasMany(r => r.Transactions)
            .WithOne(t => t.Record)
            .HasForeignKey(t => t.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);

        builder.Entity<Transaction>()
            .HasOne(t => t.Reimburses)
            .WithMany(t => t.Reimbursements)
            .HasForeignKey(t => t.ReimbursesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CategoryGroup>()
            .HasMany(g => g.Categories)
            .WithOne(c => c.Group)
            .HasForeignKey(c => c.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MachineLearning>()
            .HasOne(m => m.Category)
            .WithMany()
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Transfer>()
            .HasOne(t => t.FromRecord)
            .WithMany()
            .HasForeignKey(t => t.FromRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transfer>()
            .HasOne(t => t.ToRecord)
            .WithMany()
            .HasForeignKey(t => t.ToRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BudgetEntry>()
            .HasOne(b => b.BudgetYear)
            .WithMany()
            .HasForeignKey(b => b.BudgetYearId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BudgetEntry>()
            .HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
