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
			_ = services.AddSingleton<MainWindow>();
			_ = services.AddScoped(typeof(IPhotoRepositoryService), typeof(PhotoRepositoryService));
			_ = services.AddScoped(typeof(IConsoleCommandsService), typeof(ConsoleCommandsService));
			_ = services.AddScoped(typeof(IFocusStackingService), typeof(FocusStackingService));
			_ = services.AddScoped(typeof(IInstanceSegmentationService), typeof(InstanceSegmentationService));
			_ = services.AddScoped(typeof(IPanopticSegmentationService), typeof(PanopticSegmentationService));
			_ = services.AddScoped(typeof(IFeatureMatchingService), typeof(FeatureMatchingService));
		}
		private void OnStartup(object sender, StartupEventArgs e)
		{
			MainWindow? mainWindow = serviceProvider.GetService<MainWindow>();
			mainWindow!.Show();
		}
		protected override void OnExit(ExitEventArgs e)
		{
			IPhotoRepositoryService photoRepositoryService = serviceProvider.GetService<IPhotoRepositoryService>()!;
			IConsoleCommandsService consoleCommandsService = serviceProvider.GetService<IConsoleCommandsService>()!;
			_ = photoRepositoryService.DeleteAll();
			_ = consoleCommandsService.ClearOutputDirectory();
			base.OnExit(e);
		}
	}
}
