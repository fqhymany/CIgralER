using System.Globalization;
using Microsoft.Extensions.Logging;

namespace LawyerProject.Application.Common.Utils;
public static class DateUtils
{
    private static ILogger? _logger;
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    #region Date Conversion Methods

    /// <summary>
    /// تاریخ و ساعت فعلی را به صورت تاریخ شمسی برمی‌گرداند 
    /// </summary>
    /// <param name="format">قالب نمایش تاریخ (پیش‌فرض: yyyy/MM/dd)</param>
    public static string GetCurrentPersianDate(string format = "yyyy/MM/dd")
    {
        return ToPersianDateString(DateTime.Now, format);
    }

    /// <summary>
    /// سال جاری شمسی را برمیگرداند 
    /// </summary>
    public static string GetCurrentPersianYear()
    {
        PersianCalendar pc = new();
        int year = pc.GetYear(DateTime.Now);
        return year.ToString();
    }

    /// <summary>
    /// یک تاریخ میلادی را دریافت کرده و معادل شمسی آن را برمی‌گرداند.
    /// </summary>
    /// <param name="gregorianDate">تاریخ میلادی</param>
    /// <returns>تاریخ شمسی به فرمت 1402/11/17</returns>
    public static string ToPersianDate(DateTime gregorianDate)
    {
        return ToPersianDateString(gregorianDate, "yyyy/MM/dd");
    }

    /// <summary>
    /// یک تاریخ میلادی را دریافت کرده و معادل شمسی آن را با قالب دلخواه برمی‌گرداند.
    /// </summary>
    /// <param name="gregorianDate">تاریخ میلادی</param>
    /// <param name="format">قالب نمایش تاریخ</param>
    /// <returns>تاریخ شمسی با قالب مشخص شده</returns>
    public static string ToPersianDateString(DateTime gregorianDate, string format)
    {
        try
        {
            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(gregorianDate);
            int month = pc.GetMonth(gregorianDate);
            int day = pc.GetDayOfMonth(gregorianDate);

            string result = format
                .Replace("yyyy", year.ToString())
                .Replace("MM", month.ToString("00"))
                .Replace("M", month.ToString())
                .Replace("dd", day.ToString("00"))
                .Replace("d", day.ToString());

            // اگر قالب شامل نام روز هفته باشد
            if (format.Contains("dddd") || format.Contains("ddd"))
            {
                string dayOfWeek = GetPersianDayOfWeekName(gregorianDate);
                result = result
                    .Replace("dddd", dayOfWeek)
                    .Replace("ddd", dayOfWeek.Substring(0, 3));
            }

            // اگر قالب شامل نام ماه باشد
            if (format.Contains("MMMM") || format.Contains("MMM"))
            {
                string monthName = GetPersianMonthName(month);
                result = result
                    .Replace("MMMM", monthName)
                    .Replace("MMM", monthName.Substring(0, 3));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ToPersianDateString");
            return string.Empty;
        }
    }

    /// <summary>
    /// یک تاریخ میلادی را دریافت کرده و معادل شمسی آن را همراه با زمان برمی‌گرداند.
    /// </summary>
    /// <param name="gregorianDate">تاریخ میلادی</param>
    /// <returns>تاریخ و زمان شمسی به فرمت 1402/11/17 14:30</returns>
    public static string ToPersianDateTime(DateTime gregorianDate)
    {
        return ToPersianDateTimeString(gregorianDate, "yyyy/MM/dd HH:mm");
    }

    /// <summary>
    /// یک تاریخ میلادی را دریافت کرده و معادل شمسی آن را همراه با زمان و با قالب دلخواه برمی‌گرداند.
    /// </summary>
    /// <param name="gregorianDate">تاریخ میلادی</param>
    /// <param name="format">قالب نمایش تاریخ و زمان</param>
    /// <returns>تاریخ و زمان شمسی با قالب مشخص شده</returns>
    public static string ToPersianDateTimeString(DateTime gregorianDate, string format)
    {
        try
        {
            // ابتدا تاریخ را تبدیل می‌کنیم
            string result = ToPersianDateString(gregorianDate, format);

            // سپس زمان را جایگزین می‌کنیم
            result = result
                .Replace("HH", gregorianDate.Hour.ToString("00"))
                .Replace("H", gregorianDate.Hour.ToString())
                .Replace("mm", gregorianDate.Minute.ToString("00"))
                .Replace("m", gregorianDate.Minute.ToString())
                .Replace("ss", gregorianDate.Second.ToString("00"))
                .Replace("s", gregorianDate.Second.ToString())
                .Replace("fff", gregorianDate.Millisecond.ToString("000"));

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ToPersianDateTimeString");
            return string.Empty;
        }
    }

    /// <summary>
    /// تاریخ شمسی را به تاریخ میلادی تبدیل می‌کند.
    /// فرمت YYYY/MM/DD
    /// </summary>
    /// <param name="persianDate">تاریخ شمسی</param>
    /// <returns>تاریخ میلادی</returns>
    public static DateTime? ConvertPersianDateToGregorian(string persianDate)
    {
        try
        {
            var dateParts = persianDate.Split('/');
            if (dateParts.Length != 3)
            {
                throw new ArgumentException("Invalid Persian date format");
            }

            if (!int.TryParse(dateParts[0], out int year) ||
                !int.TryParse(dateParts[1], out int month) ||
                !int.TryParse(dateParts[2], out int day))
            {
                throw new ArgumentException("Invalid numeric values in Persian date");
            }

            // اعتبارسنجی مقادیر تاریخ شمسی
            if (!IsValidPersianDate(year, month, day))
            {
                throw new ArgumentException("Invalid Persian date values");
            }

            PersianCalendar pc = new PersianCalendar();
            return new DateTime(year, month, day, pc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting Persian date to Gregorian.");
            return null;
        }
    }

    /// <summary>
    /// تاریخ شمسی را به تاریخ میلادی تبدیل می‌کند (با ساعت و دقیقه).
    /// /// فرمت YYYY/MM/DD HH:mm
    /// </summary>
    /// <param name="persianDateTime">تاریخ و زمان شمسی</param>
    /// <returns>تاریخ و زمان میلادی</returns>
    public static DateTime? ConvertPersianDateTimeToGregorian(string persianDateTime)
    {
        try
        {
            var parts = persianDateTime.Split(' ');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid Persian date-time format");
            }

            var dateParts = parts[0].Split('/');
            if (dateParts.Length != 3)
            {
                throw new ArgumentException("Invalid Persian date format");
            }

            var timeParts = parts[1].Split(':');
            if (timeParts.Length < 2)
            {
                throw new ArgumentException("Invalid time format");
            }

            if (!int.TryParse(dateParts[0], out int year) ||
                !int.TryParse(dateParts[1], out int month) ||
                !int.TryParse(dateParts[2], out int day) ||
                !int.TryParse(timeParts[0], out int hour) ||
                !int.TryParse(timeParts[1], out int minute))
            {
                throw new ArgumentException("Invalid numeric values in Persian date-time");
            }

            int second = 0;
            if (timeParts.Length > 2 && int.TryParse(timeParts[2], out int parsedSecond))
            {
                second = parsedSecond;
            }

            // اعتبارسنجی مقادیر تاریخ و زمان
            if (!IsValidPersianDate(year, month, day) || hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
            {
                throw new ArgumentException("Invalid Persian date-time values");
            }

            PersianCalendar pc = new PersianCalendar();
            return new DateTime(year, month, day, hour, minute, second, pc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error converting Persian date-time to Gregorian: {Message}", ex.Message);
            return null;
        }
    }

    #endregion

    #region Persian Calendar Helpers

    /// <summary>
    /// بررسی می‌کند آیا تاریخ شمسی وارد شده معتبر است یا خیر
    /// </summary>
    public static bool IsValidPersianDate(int year, int month, int day)
    {
        if (year < 1 || month < 1 || month > 12 || day < 1)
            return false;

        // محاسبه حداکثر روز ماه
        int maxDayOfMonth;
        if (month <= 6)
            maxDayOfMonth = 31;
        else if (month <= 11)
            maxDayOfMonth = 30;
        else // month == 12
            maxDayOfMonth = IsLeapYear(year) ? 30 : 29;

        return day <= maxDayOfMonth;
    }

    /// <summary>
    /// بررسی می‌کند آیا سال شمسی مورد نظر، سال کبیسه است یا خیر
    /// </summary>
    public static bool IsLeapYear(int persianYear)
    {
        PersianCalendar pc = new PersianCalendar();
        try
        {
            // روش ساده: اگر سال کبیسه باشد، اسفند ۳۰ روز دارد
            return pc.GetDaysInMonth(persianYear, 12) == 30;
        }
        catch
        {
            // محاسبه بر اساس الگوریتم کبیسه‌های شمسی
            int mod = persianYear % 33;
            return mod == 1 || mod == 5 || mod == 9 || mod == 13 || mod == 17 || mod == 22 || mod == 26 || mod == 30;
        }
    }

    /// <summary>
    /// نام ماه شمسی را برمی‌گرداند
    /// </summary>
    public static string GetPersianMonthName(int month)
    {
        switch (month)
        {
            case 1: return "فروردین";
            case 2: return "اردیبهشت";
            case 3: return "خرداد";
            case 4: return "تیر";
            case 5: return "مرداد";
            case 6: return "شهریور";
            case 7: return "مهر";
            case 8: return "آبان";
            case 9: return "آذر";
            case 10: return "دی";
            case 11: return "بهمن";
            case 12: return "اسفند";
            default: return "";
        }
    }

    /// <summary>
    /// نام روز هفته در تقویم شمسی را برمی‌گرداند
    /// </summary>
    public static string GetPersianDayOfWeekName(DateTime gregorianDate)
    {
        DayOfWeek dayOfWeek = gregorianDate.DayOfWeek;
        switch (dayOfWeek)
        {
            case DayOfWeek.Saturday: return "شنبه";
            case DayOfWeek.Sunday: return "یکشنبه";
            case DayOfWeek.Monday: return "دوشنبه";
            case DayOfWeek.Tuesday: return "سه‌شنبه";
            case DayOfWeek.Wednesday: return "چهارشنبه";
            case DayOfWeek.Thursday: return "پنج‌شنبه";
            case DayOfWeek.Friday: return "جمعه";
            default: return "";
        }
    }

    #endregion

    #region Date Operations

    /// <summary>
    /// اضافه کردن تعدادی روز به تاریخ شمسی
    /// </summary>
    public static string AddDays(string persianDate, int days)
    {
        DateTime? gregorianDate = ConvertPersianDateToGregorian(persianDate);
        if (gregorianDate == null)
            return string.Empty;

        return ToPersianDate(gregorianDate.Value.AddDays(days));
    }

    /// <summary>
    /// اضافه کردن تعدادی ماه به تاریخ شمسی
    /// </summary>
    public static string AddMonths(string persianDate, int months)
    {
        DateTime? gregorianDate = ConvertPersianDateToGregorian(persianDate);
        if (gregorianDate == null)
            return string.Empty;

        return ToPersianDate(gregorianDate.Value.AddMonths(months));
    }

    /// <summary>
    /// اضافه کردن تعدادی سال به تاریخ شمسی
    /// </summary>
    public static string AddYears(string persianDate, int years)
    {
        DateTime? gregorianDate = ConvertPersianDateToGregorian(persianDate);
        if (gregorianDate == null)
            return string.Empty;

        return ToPersianDate(gregorianDate.Value.AddYears(years));
    }

    /// <summary>
    /// محاسبه اختلاف روز بین دو تاریخ شمسی
    /// </summary>
    public static int? DaysBetween(string persianDate1, string persianDate2)
    {
        DateTime? gregorianDate1 = ConvertPersianDateToGregorian(persianDate1);
        DateTime? gregorianDate2 = ConvertPersianDateToGregorian(persianDate2);

        if (gregorianDate1 == null || gregorianDate2 == null)
            return null;

        TimeSpan diff = gregorianDate2.Value - gregorianDate1.Value;
        return Math.Abs((int)diff.TotalDays);
    }

    #endregion

    #region Formatting

    /// <summary>
    /// تبدیل تاریخ شمسی به صورت متنی (مثال: دوشنبه 17 بهمن 1402)
    /// </summary>
    public static string ToLongPersianDateString(DateTime gregorianDate)
    {
        PersianCalendar pc = new PersianCalendar();
        int year = pc.GetYear(gregorianDate);
        int month = pc.GetMonth(gregorianDate);
        int day = pc.GetDayOfMonth(gregorianDate);

        string dayOfWeek = GetPersianDayOfWeekName(gregorianDate);
        string monthName = GetPersianMonthName(month);

        return $"{dayOfWeek} {day} {monthName} {year}";
    }

    /// <summary>
    /// تبدیل تاریخ شمسی به صورت متنی کوتاه (مثال: 17 بهمن 1402)
    /// </summary>
    public static string ToShortPersianDateString(DateTime gregorianDate)
    {
        PersianCalendar pc = new PersianCalendar();
        int year = pc.GetYear(gregorianDate);
        int month = pc.GetMonth(gregorianDate);
        int day = pc.GetDayOfMonth(gregorianDate);

        string monthName = GetPersianMonthName(month);

        return $"{day} {monthName} {year}";
    }

    #endregion
}
