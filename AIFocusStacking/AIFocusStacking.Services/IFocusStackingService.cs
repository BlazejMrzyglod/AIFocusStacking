namespace AIFocusStacking.Services
{
	//Interfejs serwisu odpowiedzialnego za focus stacking
	public interface IFocusStackingService
	{
		ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize, bool takeAll, int masSize, string method);
	}
}
