namespace FinanceManager.Application.Configuration;

using FinanceManager.Application.Configuration.Base;

public class IdentityConfig : IConfig
{
    public static string SectionName => "Identity";

    public bool RequireConfirmedAccount { get; set; }

    public PasswordConfig Password { get; set; } = new();
    public UserConfig User { get; set; } = new();
    public LockoutConfig Lockout { get; set; } = new();
}

public class UserConfig
{
    public bool RequireUniqueEmail { get; set; }
}

public class PasswordConfig
{
    public bool RequireDigit { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public int RequiredLength { get; set; }
}

public class LockoutConfig
{
    public int MaxFailedAccessAttempts { get; set; }
    public int DefaultLockoutTimeSpan { get; set; }
}