using AIFocusStacking.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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
			_ = services.AddScoped(typeof(IRepositoryService<string>), typeof(PhotoRepositoryService));
			_ = services.AddScoped(typeof(IRepositoryService<JArray>), typeof(JsonRepositoryService));
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
			IRepositoryService<string> photoRepositoryService = serviceProvider.GetService<IRepositoryService<string>>()!;
			IRepositoryService<JArray> jsonRepositoryService = serviceProvider.GetService<IRepositoryService<JArray>>()!;
			IConsoleCommandsService consoleCommandsService = serviceProvider.GetService<IConsoleCommandsService>()!;
			_ = photoRepositoryService.DeleteAll();
			_ = jsonRepositoryService.DeleteAll();
			_ = consoleCommandsService.ClearOutputDirectory();
			base.OnExit(e);
		}
	}
}
