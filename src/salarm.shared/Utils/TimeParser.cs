using System.Text.RegularExpressions;

namespace salarm.shared.utils;

public static class TimeParser
{
    private static readonly Regex TimeRegex = new(@"(\d+)([smhd])", RegexOptions.Compiled);

    public static TimeSpan ParseTimeString(string timeString)
    {
        var totalSeconds = 0.0;
        var matches = TimeRegex.Matches(timeString.ToLower());

        if (matches.Count == 0)
        {
            throw new ArgumentException("Invalid time format. Use format like '5s', '10m', '2h', '1d', or combinations like '4h2m'");
        }

        foreach (Match match in matches)
        {
            var value = double.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            totalSeconds += unit switch
            {
                "s" => value,
                "m" => value * 60,
                "h" => value * 3600,
                "d" => value * 86400,
                _ => throw new ArgumentException($"Invalid time unit: {unit}")
            };
        }

        return TimeSpan.FromSeconds(totalSeconds);
    }
}