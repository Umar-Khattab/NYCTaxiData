using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NYCTaxiData.Domain.Interfaces;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    // ===== Sync =====
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // ===== Async =====
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ===== Core Logic =====
    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var userId = _currentUserService.UserId ?? "System";
        var userName = _currentUserService.UserName ?? "System";
        var actor = !string.IsNullOrEmpty(userName) ? userName : userId;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var entity = entry.Entity;
            var type = entity.GetType();

            // 1. حالة الإضافة
            if (entry.State == EntityState.Added)
            {
                SetPropertySafe(entry, type, "CreatedAt", now);
                SetPropertySafe(entry, type, "CreatedBy", actor);
                SetPropertySafe(entry, type, "IsDeleted", false);
            }

            // 2. حالة التعديل
            else if (entry.State == EntityState.Modified)
            {
                SetPropertySafe(entry, type, "LastUpdatedAt", now);
                SetPropertySafe(entry, type, "LastUpdatedBy", actor);
            }

            // 3. حالة المسح (Soft Delete)
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified; // المنقذ اللي بيمنع المسح

                // بنستخدم الأسماء اللي ظاهرة في Supabase بالظبط
                SetPropertySafe(entry, type, "IsDeleted", true);
                SetPropertySafe(entry, type, "DeletedAt", now);
                SetPropertySafe(entry, type, "DeletedBy", actor);
            }
        }
    }

    // ميثود مساعدة بتعمل الـ Update بذكاء ومن غير ما تضرب الأيرور ده
    private void SetPropertySafe(EntityEntry entry, Type type, string propertyName, object value)
    {
        // بنتاكد الأول إن الـ Property موجودة في الكلاس (Reflection)
        var prop = type.GetProperty(propertyName);
        if (prop != null)
        {
            // 1. تحديث الـ Object نفسه
            prop.SetValue(entry.Entity, value);

            // 2. تحديث الـ Tracker (فقط لو الـ EF شايفها كـ Column)
            var efProp = entry.Metadata.FindProperty(propertyName);
            if (efProp != null)
            {
                entry.Property(propertyName).CurrentValue = value;
            }
        }
    }

    private void SetCreatedFields(EntityEntry entry, DateTime now, string actor)
    {
        var entity = entry.Entity;
        var type = entity.GetType();

        // بنحط القيم باستخدام الـ Reflection عشان نضمن إنها تسمع حتى لو الـ Cast فاشل
        type.GetProperty("CreatedAt")?.SetValue(entity, now);
        type.GetProperty("CreatedBy")?.SetValue(entity, actor);
        type.GetProperty("LastUpdatedAt")?.SetValue(entity, now);
        type.GetProperty("LastUpdatedBy")?.SetValue(entity, actor);
    }

    private void SetUpdatedFields(EntityEntry entry, DateTime now, string actor)
    {
        var entity = entry.Entity;
        var type = entity.GetType();

        type.GetProperty("LastUpdatedAt")?.SetValue(entity, now);
        type.GetProperty("LastUpdatedBy")?.SetValue(entity, actor);
    }

    private void HandleSoftDelete(EntityEntry entry, DateTime now, string actor)
    {
        var entity = entry.Entity;
        var type = entity.GetType();

        // بنقلب الـ State لـ Modified عشان نمنع المسح الحقيقي
        entry.State = EntityState.Modified;

        type.GetProperty("IsDeleted")?.SetValue(entity, true);
        type.GetProperty("DeletedAt")?.SetValue(entity, now);
        type.GetProperty("DeletedBy")?.SetValue(entity, actor);

        type.GetProperty("LastUpdatedAt")?.SetValue(entity, now);
        type.GetProperty("LastUpdatedBy")?.SetValue(entity, actor);
    }
}