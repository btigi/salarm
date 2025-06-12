using salarm.shared.interfaces;
using salarm.shared.models;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace salarm.cli.services;

public class NamedPipeClient : IAlarmClient
{
    public async Task<Alarm> SetAlarm(TimeSpan duration, string? soundFilePath = null, string? message = null)
    {
        using var pipeClient = new NamedPipeClientStream(".", "salarm_pipe", PipeDirection.InOut);
        
        try
        {
            await pipeClient.ConnectAsync(5000); // 5 second timeout

            var request = new Command
            {
                Action = "setalarm",
                Alarm = new Alarm
                {
                    TriggerTime = DateTime.Now.Add(duration),
                    SoundFilePath = soundFilePath,
                    Message = message
                }
            };

            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[4096];
            var bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            if (response.StartsWith("Error:"))
            {
                throw new Exception(response);
            }

            try
            {
                var alarm = JsonSerializer.Deserialize<Alarm>(response);
                if (alarm != null)
                {
                    return alarm;
                }
            }
            catch (JsonException)
            {
                throw new Exception($"Server returned invalid response: {response}");
            }
            
            throw new Exception("Failed to set alarm");
        }
        catch (TimeoutException)
        {
            throw new Exception("Could not connect to alarm service. Make sure the WPF application is running.");
        }
    }

    public async Task<IEnumerable<Alarm>> GetActiveAlarms()
    {
        using var pipeClient = new NamedPipeClientStream(".", "salarm_pipe", PipeDirection.InOut);
        
        try
        {
            await pipeClient.ConnectAsync(5000); // 5 second timeout

            var request = new Command
            {
                Action = "getactivealarms"
            };

            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[4096];
            var bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            if (response.StartsWith("Error:"))
            {
                throw new Exception(response);
            }

            try
            {
                var alarms = JsonSerializer.Deserialize<IEnumerable<Alarm>>(response);
                return alarms ?? Enumerable.Empty<Alarm>();
            }
            catch (JsonException)
            {
                throw new Exception($"Server returned invalid response: {response}");
            }
        }
        catch (TimeoutException)
        {
            throw new Exception("Could not connect to alarm service. Make sure the WPF application is running.");
        }
    }

    public async Task<bool> CancelAlarm(Guid alarmId)
    {
        using var pipeClient = new NamedPipeClientStream(".", "salarm_pipe", PipeDirection.InOut);
        
        try
        {
            await pipeClient.ConnectAsync(5000); // 5 second timeout

            var request = new Command
            {
                Action = "cancelalarm",
                AlarmId = alarmId
            };

            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[4096];
            var bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            if (response.StartsWith("Error:"))
            {
                throw new Exception(response);
            }

            return response.Contains("successfully");
        }
        catch (TimeoutException)
        {
            throw new Exception("Could not connect to alarm service. Make sure the WPF application is running.");
        }
    }
}