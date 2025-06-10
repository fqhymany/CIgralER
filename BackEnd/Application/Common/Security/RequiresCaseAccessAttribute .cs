namespace LawyerProject.Application.Common.Security;

/// <summary>
/// ویژگی برای مشخص کردن درخواست‌هایی که نیاز به بررسی دسترسی کیس دارند
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RequiresCaseAccessAttribute : Attribute
{
    public string? IdPropertyName { get; }

    public RequiresCaseAccessAttribute(string? idPropertyName = null)
    {
        IdPropertyName = idPropertyName;
    }
}
