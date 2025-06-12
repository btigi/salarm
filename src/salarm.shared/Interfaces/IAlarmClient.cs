using salarm.shared.models;

namespace salarm.shared.interfaces;

public interface IAlarmClient
{
    Task<Alarm> SetAlarm(TimeSpan duration, string? soundFilePath = null, string? message = null);
    Task<IEnumerable<Alarm>> GetActiveAlarms();
}