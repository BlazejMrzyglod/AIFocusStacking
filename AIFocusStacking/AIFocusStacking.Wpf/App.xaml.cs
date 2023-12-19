using AIFocusStacking.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AIFocusStacking.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddScoped(typeof(IPhotoRepositoryService), typeof(PhotoRepositoryService));
            services.AddScoped(typeof(IConsoleCommandsService), typeof(ConsoleCommandsService));
            services.AddScoped(typeof(IFocusStackingService), typeof(FocusStackingService));
        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            IPhotoRepositoryService photoRepositoryService = serviceProvider.GetService<IPhotoRepositoryService>();
            IConsoleCommandsService consoleCommandsService = serviceProvider.GetService<IConsoleCommandsService>();
            photoRepositoryService.DeleteAll();
            consoleCommandsService.ClearOutputDirectory();
            base.OnExit(e);
        }
    }
}
