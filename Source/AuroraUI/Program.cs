using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using AuroraUI.ViewModels;
using AuroraUI.Views;

namespace AuroraUI
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>(() => new MainWindowViewModel());
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
