using OpenCvSharp;

namespace AIFocusStacking.Services
{
	//Serwis odpowiedzialny za dobieranie wykrytych obiektów w pary
	public class FeatureMatchingService : IFeatureMatchingService
	{
		//Funkcja zwracająca liczbę pasujących kluczowych punktów między dwoma zdjęciami
		public int GetAmountOfMatches(Mat img1, Mat img2)
		{
			try
			{
				//Detektor BRISK
				BRISK briskDetector = BRISK.Create();

				//Deskryptory pierwszego zdjęcia
				Mat descriptors1 = new();

				//Wykryj kluczowe obiekty na pierwszy zdjęciu
				briskDetector.DetectAndCompute(img1, null, out KeyPoint[] keyPoints1, descriptors1);

				//Deskryptory drugiego zdjęcia
				Mat descriptors2 = new();

				//Wykryj kluczowe obiekty na drugim zdjęciu
				briskDetector.DetectAndCompute(img2, null, out KeyPoint[] keyPoints2, descriptors2);

				//Obiekt dopasowujący typu brute force
				BFMatcher bfMatcher = new(NormTypes.Hamming, false);

				//Dopasowane kluczowe punkty
				DMatch[] matches = bfMatcher.Match(descriptors1, descriptors2);

				//Zwróć liczbę dopasowanych punktów
				return matches.Length;
			}
			catch (Exception)
			{
				//Zwróć 0 jeśli wystąpił błąd
				return 0;
			}
		}
	}
}
