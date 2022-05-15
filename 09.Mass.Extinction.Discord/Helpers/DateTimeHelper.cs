namespace _09.Mass.Extinction.Discord.Helpers;

public static class DateTimeHelper
{
    private const string invalidTimeFormat = "The time you entered, is not a valid format. Use either 12 hour or 24 hour format.";
    private const string invalidYear = "The year you entered, is not valid.";
    private const string invalidMonth = "The month you entered, is not valid.";
    private const string invalidDay = "The day you entered, is not valid.";

    public static (int hour, int minute) ParseTime(string timeStr)
    {
        var timeParts = timeStr.Split(new[]
        {
            ':', '.', 'h', ' '
        }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        string? period = null;

        var firstPart = timeParts.ElementAtOrDefault(0);

        if (!int.TryParse(firstPart, out var hour))
        {
            if (firstPart?.Length > 2)
            {
                if (!int.TryParse(firstPart[..2], out hour))
                {
                    hour = int.Parse(firstPart[..1]);
                    period = firstPart[1..];
                }
                else
                {
                    period = firstPart[2..];
                }
            }
            else
            {
                throw new ArgumentException(invalidTimeFormat);
            }
        }

        if (hour > 23)
        {
            throw new ArgumentException(invalidTimeFormat);
        }

        var secondPart = timeParts.ElementAtOrDefault(1);

        if (!int.TryParse(secondPart, out var minute))
        {
            if (secondPart?.Length > 2)
            {
                minute = int.Parse(secondPart[..2]);
                period ??= secondPart[2..];
            }
            else
            {
                minute = 0;
                period ??= timeParts.ElementAtOrDefault(1);
            }
        }
        else
        {
            period ??= timeParts.ElementAtOrDefault(2);
        }

        if (minute > 59)
        {
            throw new ArgumentException(invalidTimeFormat);
        }

        if (string.IsNullOrWhiteSpace(period))
        {
            return (hour, minute);
        }

        switch (period.ToLower())
        {
            case "am":
                break;
            case "pm":
                hour += 12;
                break;
            default:
                throw new ArgumentException(invalidTimeFormat);
        }

        return (hour, minute);
    }


    public static void ValidateDate(int year, int month, int day)
    {
        if (year < 1)
        {
            throw new ArgumentException(invalidYear);
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentException(invalidMonth);
        }

        switch (month)
        {
            case 1:
            case 3:
            case 5:
            case 7:
            case 8:
            case 10:
            case 12:
                if (day is < 1 or > 31)
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
            case 2:
                if (day < 1 ||
                    (year % 4 == 0 && day > 29) || // leap years
                    (year % 4 != 0 && day > 28))
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
            case 4:
            case 6:
            case 9:
            case 11:
                if (day is < 1 or > 30)
                {
                    throw new ArgumentException(invalidDay);
                }

                break;
        }
    }

    public static string DisplayTime(TimeZoneInfo userTimeZone, int hour, int minute)
    {
        var now = DateTimeOffset.Now;

        return DisplayDateTime(userTimeZone, now.Year, now.Month, now.Day, hour, minute, "t");
    }

    public static string DisplayDateTime(TimeZoneInfo userTimeZone, int year, int month, int day, int hour, int minute, string format = "f")
    {
        Console.WriteLine("Year: {0}, Month: {1}, Day: {2}, Hour: {3}, Minute: {4}", year, month, day, hour, minute);
        var date = new DateTime(year, month, day);

        var time = new DateTimeOffset(year, month, day, hour, minute, 0, 0, userTimeZone.GetUtcOffset(date));

        return $"You entered: <t:{time.ToUnixTimeSeconds()}:{format}>";
    }
}
