using AIFocusStacking.Models;

namespace AIFocusStacking.Services
{
	public interface IPanopticSegmentationService
	{
		void RunPanopticSegmentation(List<Photo> photos);
	}
}
