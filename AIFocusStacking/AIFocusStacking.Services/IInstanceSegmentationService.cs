using AIFocusStacking.Models;

namespace AIFocusStacking.Services
{
	public interface IInstanceSegmentationService
	{
		void RunInstanceSegmentation(List<Photo> photos);
	}
}
