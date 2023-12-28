namespace AIFocusStacking.Services
{
    public interface IConsoleCommandsService
    {
        ServiceResult RunModel(string method);
        ServiceResult ClearOutputDirectory();
    }
}
