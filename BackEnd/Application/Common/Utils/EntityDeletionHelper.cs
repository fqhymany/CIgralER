using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LawyerProject.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using LawyerProject.Application.Common.Exceptions;

public static class EntityDeletionHelper
{
    /// <summary>
    /// حذف نرم رکورد از جدول با تغییر فیلد isDeleted    
    /// </summary>
    /// <param name="context">کانتکست پایگاه داده</param>
    /// <param name="tableName">نام جدول (DbSet) موجودیت اصلی</param>
    /// <param name="id">شناسه رکورد اصلی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public static async Task<bool> SoftDeleteEntityAsync(IApplicationDbContext context, string tableName, int id, CancellationToken cancellationToken)
    {
        var dbSetProperty = context.GetType().GetProperty(tableName);
        if (dbSetProperty == null)
        {
            throw new ArgumentException($"Table with name '{tableName}' was not found.");
        }

        var dbSet = dbSetProperty.GetValue(context) as IQueryable<object>;
        if (dbSet == null)
        {
            throw new InvalidOperationException($"DbSet for table '{tableName}' was not found.");
        }

        var entity = await dbSet
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);

        if (entity == null)
        {
            throw new ArgumentException($"Record with ID '{id}' was not found in table '{tableName}'.");
        }

        var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
        if (isDeletedProperty == null)
        {
            throw new InvalidOperationException($"'IsDeleted' property was not found in table '{tableName}'.");
        }

        isDeletedProperty.SetValue(entity, true);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
  
    /// <summary>
    /// حذف نرم رکورد از جدول با تغییر فیلد isDeleted  
    /// </summary>
    /// <param name="context">کانتکست پایگاه داده</param>
    /// <param name="tableName">نام جدول (DbSet) موجودیت اصلی</param>
    /// <param name="id">شناسه رکورد اصلی</param>
    /// <param name="joinForeignKeyField">نام کلید خارجی در جداول واسط (درصورت مقدار داشتن در تمامی جداول دیتابیس این فیلد رکورد جدول واسط هم حذف نرم می شود)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public static async Task<bool> SoftDeleteEntityAsync(IApplicationDbContext context,string tableName,int id,string joinForeignKeyField,CancellationToken cancellationToken)
    {
        bool deleted = await SoftDeleteEntityAsync(context, tableName, id, cancellationToken);

        if (deleted && !string.IsNullOrWhiteSpace(joinForeignKeyField))
        {
            var dbContext = context as DbContext ?? throw new InvalidOperationException("The provided context is not a DbContext.");
            IModel model = dbContext.Model;
            var joinEntityTypes = model.GetEntityTypes().Where(et => et.FindProperty(joinForeignKeyField) != null
                                                 && et.FindProperty("IsDeleted") != null);
            foreach (var joinEt in joinEntityTypes)
            {
                Type clrType = joinEt.ClrType;
                var joinSet = dbContext.GetDbSetByType(clrType);
                string fkName = joinEt.FindProperty(joinForeignKeyField)!.Name;
                var query = joinSet.Cast<object>()
                                   .Where(e => EF.Property<int>(e, fkName) == id);
                var related = await query.ToListAsync(cancellationToken);

                foreach (var row in related)
                {
                    clrType.GetProperty("IsDeleted")?.SetValue(row, true);
                    Console.WriteLine($"Soft-deleted join entity '{clrType.Name}' where '{fkName}' = {id}");
                }
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return deleted;
    }
    /// <summary>
    /// حذف نرم رکورد از جدول با تغییر فیلد isDeleted    
    ///  در صورتی که ناحیه رکورد سیستمی باشد خطا می دهد
    /// </summary>
    /// <param name="context">کانتکست پایگاه داده</param>
    /// <param name="tableName">نام جدول (DbSet) موجودیت اصلی</param>
    /// <param name="id">شناسه رکورد اصلی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public static async Task<bool> SoftDeleteProtectedSystemRecordsAsync(IApplicationDbContext context, string tableName, int id, int userRegionId, CancellationToken cancellationToken)
    {
        var dbSetProperty = context.GetType().GetProperty(tableName);
        if (dbSetProperty == null)
        {
            throw new ArgumentException($"Table with name '{tableName}' was not found.");
        }

        var dbSet = dbSetProperty.GetValue(context) as IQueryable<object>;
        if (dbSet == null)
        {
            throw new InvalidOperationException($"DbSet for table '{tableName}' was not found.");
        }

        var entity = await dbSet
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);

        if (entity == null)
        {
            throw new ArgumentException($"Record with ID '{id}' was not found in table '{tableName}'.");
        }
        var regionProp = entity.GetType().GetProperty("RegionId");
        if (regionProp != null)
        {
            object? rawValue = regionProp.GetValue(entity);
            if (rawValue is int regionValue)
            {
                if (regionValue == 0)
                {
                    throw new ForbiddenAccessException("عدم دسترسی به متغیر های سیستمی");
                }
                else if (regionValue != userRegionId)
                {
                    throw new ForbiddenAccessException("عدم دسترسی به متغیر های سایر نواحی");
                }
            }
            else
            {
                throw new InvalidOperationException("خطای مقدار RegionId");
            }
        }

        var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
        if (isDeletedProperty == null)
        {
            throw new InvalidOperationException($"'IsDeleted' property was not found in table '{tableName}'.");
        }

        isDeletedProperty.SetValue(entity, true);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
    public static IQueryable GetDbSetByType(this DbContext context, Type entityType)
    {
        var setMethod = typeof(DbContext)
            .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)
            ?? throw new InvalidOperationException("Unable to find Set<T> method on DbContext.");   
        var genericSetMethod = setMethod.MakeGenericMethod(entityType)
            ?? throw new InvalidOperationException($"Failed to make generic Set<{entityType.Name}>"); 
        var resultObj = genericSetMethod.Invoke(context, null);
        if (resultObj is not IQueryable query)
        {
            throw new InvalidOperationException($"DbContext.Set<{entityType.Name}> did not return an IQueryable");
        }
        return query;
    }


}
