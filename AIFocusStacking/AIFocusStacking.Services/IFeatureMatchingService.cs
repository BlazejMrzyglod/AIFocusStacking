using OpenCvSharp;

namespace AIFocusStacking.Services
{
	//Interfejs serwisu odpowiedzialnego za dobieranie wykrytych obiektów w pary
	public interface IFeatureMatchingService
	{
		int GetAmountOfMatches(Mat img1, Mat img2);
	}
}
