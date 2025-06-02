using System;

namespace LawyerProject.Application.Common.Utils;

public static class CastDataTypesUtils
{
    public static int ConvertGuidToInt(string guidString)
    {
        if (string.IsNullOrWhiteSpace(guidString))
            throw new ArgumentException("مقدار ورودی معتبر نیست", nameof(guidString));

        if (!Guid.TryParse(guidString, out Guid guid))
            throw new FormatException("فرمت Guid وارد شده صحیح نمی‌باشد.");

        string numericStr = guid.ToString("N");

       
        return int.Parse(numericStr);
    }
}
