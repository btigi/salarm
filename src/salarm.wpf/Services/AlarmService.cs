using salarm.shared.interfaces;
using salarm.shared.models;

namespace salarm.wpf.services
{
    public class AlarmService : IAlarmService
    {
        private readonly List<Alarm> _activeAlarms = new();
        private readonly Dictionary<Guid, System.Timers.Timer> _timers = new();

        public Task<Alarm> SetAlarm(TimeSpan duration, string? soundFilePath = null, string? message = null)
        {
            var alarm = new Alarm
            {
                Id = Guid.NewGuid(),
                TriggerTime = DateTime.Now.Add(duration),
                SoundFilePath = soundFilePath,
                Message = message ?? "Alarm!"
            };

            _activeAlarms.Add(alarm);

            var timer = new System.Timers.Timer(duration.TotalMilliseconds);
            timer.Elapsed += (sender, e) => TriggerAlarm(alarm.Id);
            timer.AutoReset = false;
            timer.Start();

            _timers[alarm.Id] = timer;

            return Task.FromResult(alarm);
        }

        public Task<IEnumerable<Alarm>> GetActiveAlarms()
        {
            return Task.FromResult(_activeAlarms.AsEnumerable());
        }

        public Task<bool> CancelAlarm(Guid alarmId)
        {
            var alarm = _activeAlarms.FirstOrDefault(a => a.Id == alarmId);
            if (alarm == null) return Task.FromResult(false);

            _activeAlarms.Remove(alarm);
            
            if (_timers.TryGetValue(alarmId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _timers.Remove(alarmId);
            }

            return Task.FromResult(true);
        }

        public Task<bool> CancelAllAlarms()
        {
            _activeAlarms.Clear();
            
            foreach (var timer in _timers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            _timers.Clear();

            return Task.FromResult(true);
        }

        private void TriggerAlarm(Guid alarmId)
        {
            var alarm = _activeAlarms.FirstOrDefault(a => a.Id == alarmId);
            if (alarm == null) return;

            alarm.IsTriggered = true;

            System.Windows.MessageBox.Show(alarm.Message, "Alarm", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            if (!string.IsNullOrEmpty(alarm.SoundFilePath))
            {
                try
                {
                    System.Media.SystemSounds.Beep.Play();
                }
                catch
                {
                    System.Media.SystemSounds.Beep.Play();
                }
            }

            _activeAlarms.Remove(alarm);
            
            if (_timers.TryGetValue(alarmId, out var timer))
            {
                timer.Dispose();
                _timers.Remove(alarmId);
            }
        }
    }
}