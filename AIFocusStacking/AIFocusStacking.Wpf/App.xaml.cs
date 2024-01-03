using AIFocusStacking.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AIFocusStacking.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddScoped(typeof(IPhotoRepositoryService), typeof(PhotoRepositoryService));
            services.AddScoped(typeof(IConsoleCommandsService), typeof(ConsoleCommandsService));
            services.AddScoped(typeof(IFocusStackingService), typeof(FocusStackingService));
			services.AddScoped(typeof(IInstanceSegmentationService), typeof(InstanceSegmentationService));
			services.AddScoped(typeof(IPanopticSegmentationService), typeof(PanopticSegmentationService));
			services.AddScoped(typeof(IFeatureMatchingService), typeof(FeatureMatchingService));
		}
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow!.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            IPhotoRepositoryService photoRepositoryService = serviceProvider.GetService<IPhotoRepositoryService>()!;
            IConsoleCommandsService consoleCommandsService = serviceProvider.GetService<IConsoleCommandsService>()!;
            photoRepositoryService.DeleteAll();
            consoleCommandsService.ClearOutputDirectory();
            base.OnExit(e);
        }
    }
}
