using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Interceptors
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly List<AuditEntry> _auditEntries = new();

        public AuditLogInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            CaptureAuditEntries(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CaptureAuditEntries(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CaptureAuditEntries(DbContext? context)
        {
            if (context == null) return;

            var userId = _currentUserService.UserId ?? "System";

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // تجاهل الـ Unchanged والـ Detached
                if (entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged) continue;

                var auditEntry = new AuditEntry
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                foreach (var property in entry.Properties)
                {
                    var propName = property.Metadata.Name;

                    if (entry.State == EntityState.Added)
                        auditEntry.NewValues[propName] = property.CurrentValue;

                    else if (entry.State == EntityState.Deleted)
                        auditEntry.OldValues[propName] = property.OriginalValue;

                    else if (entry.State == EntityState.Modified
                             && property.IsModified)
                    {
                        auditEntry.OldValues[propName] = property.OriginalValue;
                        auditEntry.NewValues[propName] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(propName);
                    }
                }

                _auditEntries.Add(auditEntry);
            }
        }
    }
}
