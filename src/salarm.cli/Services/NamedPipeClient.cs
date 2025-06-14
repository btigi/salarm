using salarm.shared.interfaces;
using salarm.shared.models;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace salarm.cli.services;

public class NamedPipeClient : IAlarmClient
{
    private const string PipeName = "salarm_pipe";
    private const int ConnectionTimeout = 5000; // 5 seconds

    private async Task<T> SendCommandAsync<T>(Command command)
    {
        using var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
        
        try
        {
            await pipeClient.ConnectAsync(ConnectionTimeout);

            var requestJson = JsonSerializer.Serialize(command);
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
                var result = JsonSerializer.Deserialize<T>(response);
                if (result == null)
                {
                    throw new Exception("Server returned null response");
                }
                return result;
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

    public async Task<Alarm> SetAlarm(TimeSpan duration, string? soundFilePath = null, string? message = null)
    {
        var command = new Command
        {
            Action = "setalarm",
            Alarm = new Alarm
            {
                TriggerTime = DateTime.Now.Add(duration),
                SoundFilePath = soundFilePath,
                Message = message
            }
        };

        return await SendCommandAsync<Alarm>(command);
    }

    public async Task<IEnumerable<Alarm>> GetActiveAlarms()
    {
        var command = new Command
        {
            Action = "getactivealarms"
        };

        var result = await SendCommandAsync<IEnumerable<Alarm>>(command);
        return result ?? Enumerable.Empty<Alarm>();
    }

    public async Task<bool> CancelAlarm(Guid alarmId)
    {
        var command = new Command
        {
            Action = "cancelalarm",
            AlarmId = alarmId
        };

        var response = await SendCommandAsync<string>(command);
        return response.Contains("successfully");
    }
}