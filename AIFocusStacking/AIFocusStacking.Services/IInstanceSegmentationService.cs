using AIFocusStacking.Models;

namespace AIFocusStacking.Services
{
	//Interfejs serwisu odpowiedzialnego za segmentacje instancji
	public interface IInstanceSegmentationService
	{
		void RunInstanceSegmentation(List<Photo> photos);
	}
}
