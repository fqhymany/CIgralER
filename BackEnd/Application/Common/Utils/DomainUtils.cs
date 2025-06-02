namespace LawyerProject.Application.Common.Utils;

public static class DomainUtils
{
    public static string? GetSubdomain(string host)
    {
        // اگر localhost باشد، null برگردان
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return null;

        // تقسیم نام دامنه به بخش‌ها
        var parts = host.Split('.');

        if (parts.Length > 2)
            return parts[0];

        return null;
    }
}
