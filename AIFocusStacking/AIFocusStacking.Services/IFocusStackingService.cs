namespace AIFocusStacking.Services
{
	public interface IFocusStackingService
	{
		ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize, bool takeAll, int masSize, string method);
	}
}
