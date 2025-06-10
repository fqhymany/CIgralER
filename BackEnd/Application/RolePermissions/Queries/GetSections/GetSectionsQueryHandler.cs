using MediatR;
using LawyerProject.Domain.Constants;

namespace LawyerProject.Application.RolePermissions.Queries.GetSections;

public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, string[]>
{
    private static readonly Dictionary<string, string> SectionTranslations = new()
    {
        ["UserManagement"] = "مدیریت کاربران",
        ["CaseManagement"] = "مدیریت پرونده‌ها",
        ["Reports"] = "گزارشات",
        ["Financial"] = "مالی",
        ["FileManagement"] = "مدیریت فایل",
        ["Notification"] = "اطلاع‌رسانی",
        ["Support"] = "پشتیبانی",
        ["Settings"] = "تنظیمات",
        ["RegionManagement"] = "مدیریت مناطق",
        ["RolePermission"] = "مدیریت نقش و دسترسی",
        ["ClientManagement"] = "مدیریت موکلین",
        ["LawyerManagement"] = "مدیریت وکلا",
        ["JudicialActions"] = "اقدامات قضایی",
        ["CourtManagement"] = "مدیریت دادگاه",
        ["Payment"] = "پرداخت‌ها",
        ["AuditLog"] = "گزارش تغییرات"
    };

    public Task<string[]> Handle(GetSectionsQuery request, CancellationToken cancellationToken)
    {
        var sections = typeof(Sections)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)?.ToString())
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => SectionTranslations.TryGetValue(s!, out var fa) ? fa : s) // ترجمه یا مقدار اصلی
            .ToArray();

        return Task.FromResult(sections)!;
    }
}
