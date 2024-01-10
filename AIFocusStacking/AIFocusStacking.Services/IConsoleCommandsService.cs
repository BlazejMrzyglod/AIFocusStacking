namespace AIFocusStacking.Services
{
	//Interfejs serwisu uruchamiającego komendy w konsoli
	public interface IConsoleCommandsService
    {
        ServiceResult RunModel(string method, string confidence);
        ServiceResult ClearOutputDirectory();
    }
}
