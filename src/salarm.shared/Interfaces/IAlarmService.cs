using salarm.shared.models;

namespace salarm.shared.interfaces;

public interface IAlarmService
{
    Task<Alarm> SetAlarm(TimeSpan duration, string? soundFilePath = null, string? message = null);
    Task<IEnumerable<Alarm>> GetActiveAlarms();
    Task<bool> CancelAlarm(Guid alarmId);
    Task<bool> CancelAllAlarms();
}