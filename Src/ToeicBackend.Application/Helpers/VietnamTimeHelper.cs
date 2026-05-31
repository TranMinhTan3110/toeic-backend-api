using System.Globalization;

namespace ToeicBackend.Application.Helpers;

public static class VietnamTimeHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public static DateTime GetNowInVietnam()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
    }

    public static string GetTodayDateKey()
    {
        return GetNowInVietnam().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string GetWeeklyPeriodKey(DateTime? vnTime = null)
    {
        var local = vnTime ?? GetNowInVietnam();
        var year = ISOWeek.GetYear(local);
        var week = ISOWeek.GetWeekOfYear(local);
        return $"{year}-W{week:D2}";
    }

    public static int GetDaysBetweenDateKeys(string fromDateKey, string toDateKey)
    {
        var from = DateTime.ParseExact(fromDateKey, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var to = DateTime.ParseExact(toDateKey, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return (int)(to.Date - from.Date).TotalDays;
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
