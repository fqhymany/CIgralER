using LawyerProject.Application.Common.Exceptions;
using System.Reflection;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.Common.Behaviours;
/// <summary>
/// Pipeline behavior برای بررسی دسترسی کاربر به کیس‌ها
/// این کلاس به صورت خودکار تمام درخواست‌های مربوط به کیس را بررسی می‌کند
/// </summary>
public class CaseAccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private readonly ICaseAccessFilter _accessFilter;

    public CaseAccessBehavior(ICaseAccessFilter accessFilter)
    {
        _accessFilter = accessFilter;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // بررسی آیا درخواست دارای ویژگی RequiresCaseAccess است
        var requiresCaseAccessAttribute = request.GetType().GetCustomAttribute<RequiresCaseAccessAttribute>();
        if (requiresCaseAccessAttribute == null)
        {
            // اگر ویژگی وجود ندارد، بدون بررسی دسترسی ادامه می‌دهیم
            return await next();
        }

        // دریافت مقدار ID کیس از درخواست
        int caseId = GetCaseIdFromRequest(request, requiresCaseAccessAttribute.IdPropertyName);

        // بررسی دسترسی کاربر به کیس
        if (!await _accessFilter.HasUserAccessToCaseAsync(caseId, cancellationToken))
        {
            throw new ForbiddenAccessException($"کاربر به پرونده دسترسی ندارد.");
        }

        // ادامه پایپلاین و اجرای هندلر اصلی
        return await next();
    }

    /// <summary>
    /// استخراج شناسه کیس از درخواست با استفاده از Convention یا Reflection
    /// </summary>
    private int GetCaseIdFromRequest(TRequest request, string? idPropertyName)
    {
        // اگر نام پراپرتی مشخص شده باشد، از آن استفاده می‌کنیم
        if (!string.IsNullOrEmpty(idPropertyName))
        {
            var customProperty = request.GetType().GetProperty(idPropertyName);
            if (customProperty != null && customProperty.PropertyType == typeof(int))
            {
                object? value = customProperty.GetValue(request);
                if (value != null)
                {
                    return (int)value;
                }
            }
        }

        // روش اول: بررسی اگر درخواست از ICaseRequest پیروی می‌کند
        if (request is ICaseRequest caseRequest)
        {
            return caseRequest.CaseId;
        }

        // روش دوم: بررسی پراپرتی با اسم Id یا CaseId
        var idProperty = request.GetType().GetProperty("Id") ??
                         request.GetType().GetProperty("CaseId");

        if (idProperty != null && idProperty.PropertyType == typeof(int))
        {
            object? value = idProperty.GetValue(request);
            if (value != null)
            {
                return (int)value;
            }
        }

        // روش سوم: بررسی فیلد Case و سپس Id آن
        var caseProperty = request.GetType().GetProperty("Case");
        if (caseProperty != null)
        {
            object? caseObject = caseProperty.GetValue(request);
            if (caseObject != null)
            {
                var caseIdProperty = caseObject.GetType().GetProperty("Id");
                if (caseIdProperty != null && caseIdProperty.PropertyType == typeof(int))
                {
                    object? caseIdValue = caseIdProperty.GetValue(caseObject);
                    if (caseIdValue != null)
                    {
                        return (int)caseIdValue;
                    }
                }
            }
        }

        throw new InvalidOperationException(
            $"نمی‌توان شناسه کیس را از درخواست {request.GetType().Name} استخراج کرد. " +
            "لطفاً از ICaseRequest پیروی کنید یا از ویژگی RequiresCaseAccess با پارامتر استفاده کنید."
        );
    }
}
