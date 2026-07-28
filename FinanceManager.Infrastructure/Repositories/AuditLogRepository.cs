using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace FinanceManager.Infrastructure.Repositories;

public class AuditLogRepository(FinanceManagerDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IAuditLogRepository
{
    public Task LogAsync(Guid userId, AuditedEvent action, string? description, DateTime date)
    {
        var httpContext = httpContextAccessor.HttpContext;

        var ipAddress = httpContext is null ? null : GetIpAddress(httpContext);
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Event = action,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Date = DateTime.UtcNow
        };

        dbContext.AuditLogs.Add(log); 
        return Task.CompletedTask;
    }

    private static string? GetIpAddress(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
