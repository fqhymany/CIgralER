namespace LawyerProject.Domain.Constants;

public abstract class Roles
{
    public const string Administrator = nameof(Administrator);//مدیر سیستم
    public const string RegionAdmin = nameof(RegionAdmin);//مدیر ناحیه
    public const string Lawyer = nameof(Lawyer);//وکیل
    //public const string LawyerAssistant = nameof(LawyerAssistant);//دستیار وکیل
    public const string Secretary = nameof(Secretary);//منشی
    public const string Client = nameof(Client);//موکل
    public const string Express = nameof(Express);//مسئول پرونده
    public const string Litigant = nameof(Litigant);//طرف دعوی
}
