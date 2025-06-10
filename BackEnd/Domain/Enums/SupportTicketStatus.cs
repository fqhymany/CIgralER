namespace LawyerProject.Domain.Enums;

public enum SupportTicketStatus
{
    Open = 1, // تازه ایجاد شده، منتظر اپراتور
    Assigned = 2, // به یک اپراتور تخصیص داده شده
    PendingAgentResponse = 3, // کاربر پیام داده، منتظر پاسخ اپراتور
    PendingUserResponse = 4, // اپراتور پیام داده، منتظر پاسخ کاربر
    PendingClosureByUser = 5, // کاربر درخواست بسته شدن داده، منتظر تایید اپراتور
    ClosedByAgent = 6, // توسط اپراتور بسته شده
    ClosedByUser = 7, // توسط کاربر بسته شده (پس از تایید اپراتور یا خودکار)
    ClosedAutomatically = 8 // به طور خودکار بسته شده (مثلا پس از عدم فعالیت طولانی یا عدم تایید اپراتور)
}
