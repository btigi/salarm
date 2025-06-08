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

    static void ShowHelp()
    {
        Console.WriteLine("Usage: salarm [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  -t, --time <time>     Time until alarm (e.g., 5s, 10m, 2h, 1d, or 4h2m)");
        Console.WriteLine("  -f, --file <file>     Sound file to play (MP3 format)");
        Console.WriteLine("  -m, --message <text>  Message to display (max 500 characters)");
        Console.WriteLine("  -h, --help            Show this help message");
    }
}