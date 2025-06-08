using Microsoft.Extensions.DependencyInjection;
using salarm.shared.interfaces;

namespace salarm.wpf.services
{
    public class SystemTrayIcon : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly IAlarmService _alarmService;

        public SystemTrayIcon(IServiceProvider serviceProvider)
        {
            _alarmService = serviceProvider.GetRequiredService<IAlarmService>();
            
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "salarm"
            };

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());

            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                }
            };
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}