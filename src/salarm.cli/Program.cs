using salarm.cli.services;
using salarm.shared.utils;

namespace salarm.cli;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            // Check for list command first
            if (args[0].ToLower() == "list" || args[0].ToLower() == "-l" || args[0].ToLower() == "--list")
            {
                await HandleListCommand();
                return;
            }

            // Check for cancel command
            if (args[0].ToLower() == "cancel" && args.Length > 1)
            {
                await HandleCancelCommand(args[1]);
                return;
            }

            string? timeArg = null;
            string? soundFile = null;
            string? message = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-t":
                    case "--time":
                        if (i + 1 < args.Length)
                            timeArg = args[++i];
                        break;
                    case "-f":
                    case "--file":
                        if (i + 1 < args.Length)
                            soundFile = args[++i];
                        break;
                    case "-m":
                    case "--message":
                        if (i + 1 < args.Length)
                            message = args[++i];
                        break;
                    case "-h":
                    case "--help":
                        ShowHelp();
                        return;
                }
            }

            if (string.IsNullOrEmpty(timeArg))
            {
                Console.WriteLine("Error: Time parameter (-t) is required.");
                ShowHelp();
                return;
            }

            if (message?.Length > 500)
            {
                Console.WriteLine("Error: Message cannot be longer than 500 characters.");
                return;
            }

            var duration = TimeParser.ParseTimeString(timeArg);
            var client = new NamedPipeClient();
            
            try
            {
                var alarm = await client.SetAlarm(duration, soundFile, message);
                Console.WriteLine($"Alarm set successfully (ID: {alarm.Id})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task HandleListCommand()
    {
        try
        {
            var client = new NamedPipeClient();
            var alarms = await client.GetActiveAlarms();
            
            if (!alarms.Any())
            {
                Console.WriteLine("No pending alarms.");
                return;
            }

            Console.WriteLine("Pending alarms:");
            Console.WriteLine("================");
            
            foreach (var alarm in alarms.OrderBy(a => a.TriggerTime))
            {
                var timeRemaining = alarm.TriggerTime - DateTime.Now;
                var timeRemainingStr = FormatTimeSpan(timeRemaining);
                
                Console.WriteLine($"ID: {alarm.Id}");
                Console.WriteLine($"  Trigger Time: {alarm.TriggerTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"  Time Remaining: {timeRemainingStr}");
                Console.WriteLine($"  Message: {alarm.Message}");
                
                if (!string.IsNullOrEmpty(alarm.SoundFilePath))
                {
                    Console.WriteLine($"  Sound File: {alarm.SoundFilePath}");
                }
                
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task HandleCancelCommand(string partialGuid)
    {
        try
        {
            var client = new NamedPipeClient();
            var alarms = await client.GetActiveAlarms();
            
            if (!alarms.Any())
            {
                Console.WriteLine("No pending alarms to cancel.");
                return;
            }

            // Find alarms that match the partial GUID
            var matchingAlarms = alarms.Where(a => 
                a.Id.ToString().StartsWith(partialGuid, StringComparison.OrdinalIgnoreCase) ||
                a.Id.ToString().Replace("-", "").StartsWith(partialGuid, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (matchingAlarms.Count == 0)
            {
                Console.WriteLine($"No alarm found with ID starting with '{partialGuid}'.");
                Console.WriteLine("Use 'salarm list' to see all pending alarms.");
                return;
            }

            if (matchingAlarms.Count > 1)
            {
                Console.WriteLine($"Multiple alarms found starting with '{partialGuid}':");
                Console.WriteLine("Please be more specific. Matching alarms:");
                
                foreach (var alarm in matchingAlarms.OrderBy(a => a.TriggerTime))
                {
                    var timeRemaining = alarm.TriggerTime - DateTime.Now;
                    var timeRemainingStr = FormatTimeSpan(timeRemaining);
                    Console.WriteLine($"  {alarm.Id} - {alarm.Message} ({timeRemainingStr} remaining)");
                }
                return;
            }

            // Cancel the single matching alarm
            var alarmToCancel = matchingAlarms.First();
            var success = await client.CancelAlarm(alarmToCancel.Id);
            
            if (success)
            {
                Console.WriteLine($"Alarm cancelled successfully (ID: {alarmToCancel.Id})");
                Console.WriteLine($"Message: {alarmToCancel.Message}");
            }
            else
            {
                Console.WriteLine("Failed to cancel alarm.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalMilliseconds < 0)
            return "Overdue";

        var parts = new List<string>();
        
        if (timeSpan.Days > 0)
            parts.Add($"{timeSpan.Days}d");
        if (timeSpan.Hours > 0)
            parts.Add($"{timeSpan.Hours}h");
        if (timeSpan.Minutes > 0)
            parts.Add($"{timeSpan.Minutes}m");
        if (timeSpan.Seconds > 0 && timeSpan.TotalHours < 1)
            parts.Add($"{timeSpan.Seconds}s");

        return parts.Any() ? string.Join(" ", parts) : "Less than 1s";
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage: salarm [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  list, -l, --list         Show pending alarms");
        Console.WriteLine("  cancel <partial-id>       Cancel an alarm by partial GUID");
        Console.WriteLine();
        Console.WriteLine("Set Alarm Options:");
        Console.WriteLine("  -t, --time <time>        Time until alarm (e.g., 5s, 10m, 2h, 1d, or 4h2m)");
        Console.WriteLine("  -f, --file <file>        Sound file to play (MP3 format)");
        Console.WriteLine("  -m, --message <text>     Message to display (max 500 characters)");
        Console.WriteLine("  -h, --help               Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  salarm -t 5m -m \"Take a break\"");
        Console.WriteLine("  salarm list");
        Console.WriteLine("  salarm cancel 12ab34cd");
        Console.WriteLine("  salarm cancel 12        # Cancel alarm starting with '12'");
        Console.WriteLine("  salarm -t 1h -f alarm.mp3 -m \"Meeting reminder\"");
    }
}