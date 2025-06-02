using LawyerProject.Application.Common.Interfaces;
using System.Linq.Expressions;

namespace LawyerProject.Application.Common.Security;

public static class RegionAccessQueryFilter
{
    public static IQueryable<T> FilterByUserRegionAccess<T>(
        this IQueryable<T> query,
        IApplicationDbContext dbContext,
        string userId,
        List<int>? accessibleRegionIds,
        Expression<Func<T, int?>> regionIdSelector)
    {
        if (accessibleRegionIds == null || !accessibleRegionIds.Any())
            return query.Take(0); // هیچ دسترسی وجود ندارد

        ParameterExpression parameter = regionIdSelector.Parameters[0];
        MemberExpression? memberAccess = regionIdSelector.Body as MemberExpression;

        // ایجاد عبارت: entity => accessibleRegionIds.Contains(entity.RegionId)
        var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
        var accessibleRegionsConstant = Expression.Constant(accessibleRegionIds);

        var nullCheck = Expression.NotEqual(memberAccess!, Expression.Constant(null, typeof(int?)));
        var valueAccess = Expression.Property(memberAccess!, "Value");
        var contains = Expression.Call(accessibleRegionsConstant, containsMethod!, valueAccess);

        var condition = Expression.AndAlso(nullCheck, contains);
        var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);

        return query.Where(lambda);
    }
}
