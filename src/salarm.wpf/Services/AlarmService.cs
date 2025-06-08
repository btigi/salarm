using salarm.shared.interfaces;
using salarm.shared.models;
using NAudio.Wave;
using System.IO;

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

            if (!string.IsNullOrEmpty(alarm.SoundFilePath))
            {
                Task.Run(() =>
                {
                    try
                    {
                        PlayAudioFile(alarm.SoundFilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Failed to play audio file '{alarm.SoundFilePath}': {ex.Message}");
                        System.Media.SystemSounds.Beep.Play();
                    }
                });
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }

            System.Windows.MessageBox.Show(alarm.Message, "Alarm", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            _activeAlarms.Remove(alarm);
            
            if (_timers.TryGetValue(alarmId, out var timer))
            {
                timer.Dispose();
                _timers.Remove(alarmId);
            }
        }

        private void PlayAudioFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"Audio file not found: {filePath}");
            }

            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            
            switch (extension)
            {
                case ".mp3":
                    PlayMp3File(filePath);
                    break;
                case ".wav":
                    PlayWavFile(filePath);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported audio format: {extension}");
            }
        }

        private void PlayMp3File(string filePath)
        {
            var audioFile = new AudioFileReader(filePath);
            var outputDevice = new WaveOutEvent();
            
            outputDevice.PlaybackStopped += (sender, e) =>
            {
                outputDevice.Dispose();
                audioFile.Dispose();
            };
            
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        private void PlayWavFile(string filePath)
        {
            var player = new System.Media.SoundPlayer(filePath);
            player.LoadAsync();
            player.Play();
        }
    }
}