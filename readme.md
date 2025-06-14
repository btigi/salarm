# salarm

A system tray alarm application with a command-line interface for setting alarms.

## Features

- System tray application that runs in the background
- Command-line interface for setting alarms
- Support for custom sound files
- Custom alarm messages
- Flexible time format (seconds, minutes, hours, days)

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/salarm

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Usage

1. First, start the WPF application:
```
dotnet run --project salarm.Wpf
```

2. Then set alarms using the command-line interface:
```
# Set an alarm for 5 seconds
salarm -t 5s

# Set an alarm for 10 minutes with a custom sound
salarm -t 10m -f beep.mp3

# Set an alarm for 2 hours with a message
salarm -t 2h -m "Time to take a break!"

# Set an alarm for 4 hours and 30 minutes
salarm -t 4h30m
```

### Command-line Options

- `list, -l, --list`: List pending alarms
- `cancel <partial-id>`: Cancel an alarm by partial GUID
- `-t, --time <time>`: Time until alarm (required)
  - Supported units: s (seconds), m (minutes), h (hours), d (days)
  - Examples: 5s, 10m, 2h, 1d, 4h2m
- `-f, --file <file>`: Sound file to play (MP3 format)
- `-m, --message <text>`: Message to display (max 500 characters)
- `-h, --help`: Show help message

## Configuration

The WPF application uses `appsettings.json` for configuration:

```json
{
  "DefaultSoundPath": "sounds/default.mp3",
  "NotificationSettings": {
    "ShowAlarmSet": true,
    "ShowAlarmTriggered": true
  }
}
```

## Notes

- The WPF application must be running for the command-line interface to work
- Sound files must be in MP3 format
- Messages are limited to 500 characters
- The application will show a system tray notification when an alarm is triggered