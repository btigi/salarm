using Microsoft.Extensions.DependencyInjection;
using salarm.shared.interfaces;
using System.IO;
using Forms = System.Windows;

namespace salarm.wpf.services
{
    public class SystemTrayIcon : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Icon _appIcon;

        public SystemTrayIcon(IServiceProvider serviceProvider)
        {            
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            _appIcon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;
            
            _notifyIcon = new NotifyIcon
            {
                Icon = _appIcon,
                Visible = true,
                Text = "Salarm"
            };

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Forms.Application.Current.Shutdown());

            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (Forms.Application.Current.MainWindow != null)
                {
                    Forms.Application.Current.MainWindow.Show();
                    Forms.Application.Current.MainWindow.WindowState = Forms.WindowState.Normal;
                }
            };
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
            _appIcon.Dispose();
        }
    }
}