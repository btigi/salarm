using Microsoft.Extensions.DependencyInjection;
using salarm.shared.interfaces;
using salarm.wpf.services;

namespace salarm.wpf
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider? _serviceProvider;
        private SystemTrayIcon? _systemTrayIcon;
        private NamedPipeServer? _pipeServer;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            var services = new ServiceCollection();
            
            services.AddSingleton<IAlarmService, AlarmService>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            _systemTrayIcon = new SystemTrayIcon(_serviceProvider);
            _pipeServer = new NamedPipeServer(_serviceProvider);
            _ = _pipeServer.StartAsync();
            
            base.OnStartup(e);
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            _systemTrayIcon?.Dispose();
            _pipeServer?.Dispose();
            base.OnExit(e);
        }
    }
}