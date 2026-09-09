namespace FinanceManager.Infrastructure.Data;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Entities.Base;
using FinanceManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public sealed class FinanceManagerDbContext(DbContextOptions<FinanceManagerDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
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

    public Guid? CurrentUserId = null;
    public void SetCurrentUserId(Guid? userId) 
    {
        CurrentUserId = userId;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentUserId.HasValue)
        {
            foreach (var entry in ChangeTracker.Entries<UserOwnedEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.UserId = CurrentUserId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentityTables(builder);
        ConfigureRelationships(builder);
        ConfigureIdentityIndexes(builder);
        ConfigureIdentityFilters(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }

    private static void ConfigureRelationships(ModelBuilder builder)
    {
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
            .OnDelete(DeleteBehavior.NoAction);

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


    private static void ConfigureIdentityIndexes(ModelBuilder builder)
    {
        foreach (var type in GetIdentityEntityTypes(builder))
        {
            builder.Entity(type).HasIndex(nameof(UserOwnedEntity.UserId));
        }
    }

    private void ConfigureIdentityFilters(ModelBuilder builder)
    {
        var configureMethod = typeof(FinanceManagerDbContext).GetMethod(nameof(ConfigureIdentityFilter), BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var type in GetIdentityEntityTypes(builder))
        {
            var genericMethod = configureMethod?.MakeGenericMethod(type);
            genericMethod?.Invoke(this, [builder]);
        }
    }

    private static IEnumerable<Type> GetIdentityEntityTypes(ModelBuilder builder)
    {
        return builder.Model.GetEntityTypes()
            .Select(e => e.ClrType)
            .Where(t => typeof(UserOwnedEntity).IsAssignableFrom(t) && t != typeof(UserOwnedEntity));
    }

    private void ConfigureIdentityFilter<TEntity>(ModelBuilder builder) where TEntity : UserOwnedEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(t => CurrentUserId != null && t.UserId == CurrentUserId);
    }
}
