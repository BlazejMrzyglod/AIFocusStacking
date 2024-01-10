using AIFocusStacking.Models;

namespace AIFocusStacking.Services
{
	//Interfejs serwisu odpowiedzialnego za panoptyczną segmentacje
	public interface IPanopticSegmentationService
	{
		void RunPanopticSegmentation(List<Photo> photos, string confidence);
	}
}
