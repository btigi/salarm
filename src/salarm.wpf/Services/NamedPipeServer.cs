using Microsoft.Extensions.DependencyInjection;
using salarm.shared.interfaces;
using salarm.shared.models;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace salarm.wpf.services
{
    public class NamedPipeServer : IDisposable
    {
        private readonly IAlarmService _alarmService;
        private NamedPipeServerStream? _pipeServer;
        private bool _isRunning;

        public NamedPipeServer(IServiceProvider serviceProvider)
        {
            _alarmService = serviceProvider.GetRequiredService<IAlarmService>();
        }

        public async Task StartAsync()
        {
            _isRunning = true;
            
            while (_isRunning)
            {
                _pipeServer = new NamedPipeServerStream("salarm_pipe", PipeDirection.InOut);
                
                try
                {
                    await _pipeServer.WaitForConnectionAsync();
                    await HandleClientAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pipe server error: {ex.Message}");
                }
                finally
                {
                    _pipeServer?.Dispose();
                }
            }
        }

        private async Task HandleClientAsync()
        {
            if (_pipeServer == null) return;

            var buffer = new byte[1024];
            var bytesRead = await _pipeServer.ReadAsync(buffer, 0, buffer.Length);
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var response = await ProcessMessageAsync(message);
            var responseBytes = Encoding.UTF8.GetBytes(response);
            await _pipeServer.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        private async Task<string> ProcessMessageAsync(string message)
        {
            try
            {
                var command = JsonSerializer.Deserialize<Command>(message);
                if (command == null) return "Error: Invalid command";

                switch (command.Action.ToLower())
                {
                    case "setalarm":
                    case "add":
                        if (command.Alarm != null)
                        {
                            var duration = command.Alarm.TriggerTime - DateTime.Now;
                            var alarm = await _alarmService.SetAlarm(duration, command.Alarm.SoundFilePath, command.Alarm.Message);
                            return JsonSerializer.Serialize(alarm);
                        }
                        return "Error: No alarm data provided";

                    case "getactivealarms":
                    case "list":
                        var alarms = await _alarmService.GetActiveAlarms();
                        return JsonSerializer.Serialize(alarms);

                    case "cancelalarm":
                    case "remove":
                        if (command.AlarmId.HasValue)
                        {
                            var success = await _alarmService.CancelAlarm(command.AlarmId.Value);
                            return success ? "Alarm cancelled successfully" : "Alarm not found";
                        }
                        return "Error: No alarm ID provided";

                    case "cancelallalarms":
                        var cancelAllSuccess = await _alarmService.CancelAllAlarms();
                        return cancelAllSuccess ? "All alarms cancelled successfully" : "Failed to cancel alarms";

                    default:
                        return "Error: Unknown command";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _pipeServer?.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}