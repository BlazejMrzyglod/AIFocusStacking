using AIFocusStacking.Models;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	//Serwis odpowiedzialny za focus stacking
	public class FocusStackingService : IFocusStackingService
	{
		//Serwis odpowiedzialny za segmentacje instancji
		protected readonly IInstanceSegmentationService _instanceSegmentationService;
		//Serwis odpowiedzialny za panoptyczną segmentacje
		protected readonly IPanopticSegmentationService _panopticSegmentationService;
		//Lista zdjęć
		protected List<Photo> _photos;

		public FocusStackingService(IInstanceSegmentationService instanceSegmentationService, IPanopticSegmentationService panopticSegmentationService)
		{
			_instanceSegmentationService = instanceSegmentationService;
			_panopticSegmentationService = panopticSegmentationService;
			_photos = new();
		}

		//Funkcja odpowiedzialna za wyrównywanie zdjęć
		//Parametry: zdjęcie referencyjne, zdjęcie do wyrównania
		private static Mat AlignImages(Mat referenceImage, Mat currentImage)
		{
			//Zamień zdjęcia na odcień szarości
			Mat grayReference = new();
			Mat grayCurrent = new();
			Cv2.CvtColor(referenceImage, grayReference, ColorConversionCodes.BGR2GRAY);
			Cv2.CvtColor(currentImage, grayCurrent, ColorConversionCodes.BGR2GRAY);

			//Parametry wyrównania
			Mat warpMatrix = Mat.Eye(2, 3, MatType.CV_32F);
			TermCriteria criteria = new(CriteriaTypes.Count | CriteriaTypes.Eps, 500, 1e-10);

			//Konwertuj zdjęcia do odpowiedniego formatu
			grayReference.ConvertTo(grayReference, MatType.CV_8UC1);
			grayCurrent.ConvertTo(grayCurrent, MatType.CV_8UC1);

			//Wykonaj transformacji ECC
			try
			{
				_ = Cv2.FindTransformECC(grayReference, grayCurrent, warpMatrix, MotionTypes.Euclidean, criteria);
			}
			//Ochrona przed wyjątkami
			//TODO: Poprawić i dodać komunikat
			catch
			{
				return currentImage;
			}

			//Wyrównaj zdjęcia
			Mat alignedImage = new();
			Cv2.WarpAffine(currentImage, alignedImage, warpMatrix, referenceImage.Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);

			return alignedImage;
		}

		//Funckja wykonująca focus stacking
		public ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize,
											  bool takeAll, int maskSize, string method, string confidence)
		{
			ServiceResult serviceResult = new();
			try
			{
				Directory.CreateDirectory("outputImages");
				//Co najmniej dwa zdjęcia wymagane
				if (photos.Count() < 2)
				{
					throw new Exception("Dodaj co najmniej dwa zdjęcia");
				}

				_photos = new();
				//Ustaw pierwsze zdjęcie z listy jako zdjęcie referencyjne i dodaj do kolekcji
				Mat referenceImage = new(photos.First(), ImreadModes.Unchanged);
				_photos.Add(new Photo(referenceImage, photos.First().Split("\\").Last()));

				//Dodaj pozostałe zdjęcia do kolekcji i ewentualnie wyrównaj
				for (int i = 1; i < photos.Count(); i++)
				{
					//Aktualne zdjęcie
					Mat currentImage = new(photos.ToArray()[i], ImreadModes.Unchanged);

					//Dokonaj ewentualnego wyrównania
					if (alignment)
					{
						Mat alignedImage = AlignImages(referenceImage, currentImage);
						_photos.Add(new Photo(alignedImage, photos.ToArray()[i].Split("\\").Last()));
					}
					else
					{
						_photos.Add(new Photo(currentImage, photos.ToArray()[i].Split("\\").Last()));
					}
				}

				Mat matGauss = new();

				//Użyj filtrów na wszystkich zdjęciach
				for (int i = 0; i < _photos.Count; i++)
				{
					//Ewentualne użycie filtru Gaussa
					if (gauss)
					{
						Cv2.GaussianBlur(_photos[i].Matrix, matGauss, new Size() { Height = gaussSize, Width = gaussSize }, 0);
					}
					else
					{
						_photos[i].Matrix.CopyTo(matGauss);
					}

					//Użycie filtru Laplace'a
					Cv2.CvtColor(matGauss, matGauss, ColorConversionCodes.BGR2GRAY);
					Mat matLaplace = new();
					Cv2.Laplacian(matGauss, matLaplace, -1, laplaceSize);

					//Zapisanie zdjęcia po filtrowaniu
					_photos[i].MatrixAfterLaplace = matLaplace;
					_ = matLaplace.SaveImage($"outputImages\\laplace{i}.jpg");

				}

				//Zależnie od metody użyj segmentacji zdjęcia
				if (method == "2")
				{
					_instanceSegmentationService.RunInstanceSegmentation(_photos, confidence);
				}
				else if (method == "3")
				{
					_panopticSegmentationService.RunPanopticSegmentation(_photos, confidence);
				}

				//Ustawienie parametrów początkowych
				Mat result = _photos.First().Matrix.Clone();
				byte maxIntensity = 0;
				byte intensity = 0;

				//Dokonaj wyboru najlepszych pikseli zależnie od wybranej formy
				if (!takeAll)
				{
					TakeSinglePixel(result, ref maxIntensity, ref intensity, maskSize);
				}
				else
				{
					TakeAllPixels(result, ref maxIntensity, ref intensity, maskSize);
				}

				//Zapisz zdjęcie
				_ = result.SaveImage("outputImages\\result." + _photos.First().Name.Split(".").Last());

				serviceResult.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				serviceResult.Result = ServiceResultStatus.Error;
				serviceResult.Messages.Add(e.Message);
			}

			return serviceResult;

		}

		//Funkcja wybierająca do zdjęcia końcowego najlepsze obszary w całości
		private void TakeAllPixels(Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			//Iteruj po obszarach na zdjęciu, których wielkość jest określona parametrem maskSize
			for (int i = maskSize; i < result.Rows - maskSize; i += maskSize)
			{
				for (int j = maskSize; j < result.Cols - maskSize; j += maskSize)
				{
					//Iteruj po wszystkich zdjęciach
					maxIntensity = 0;
					for (int z = 0; z < _photos.Count; z++)
					{
						intensity = 0;

						//Sprawdź intensywność określonego obszaru
						for (int x = -maskSize; x <= maskSize; x++)
						{
							for (int y = -maskSize; y <= maskSize; y++)
							{
								intensity += _photos[z].MatrixAfterLaplace!.At<byte>(i + x, j + y);
							}
						}

						//Jeśli obszar na danym zdjęciu ma największą intensywność, użyj na zdjęciu docelowym tego obszaru
						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							for (int x = -maskSize; x <= maskSize; x++)
							{
								for (int y = -maskSize; y <= maskSize; y++)
								{
									result.At<Vec3b>(i + x, j + y) = _photos[z].Matrix.At<Vec3b>(i + x, j + y);
								}
							}

						}
					}

				}
			}
		}

		//Funkcja wybierająca do zdjęcia końcowego najlepszy pojedynczy piksel
		private void TakeSinglePixel(Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			//Iteruj po wszystkich pikselach zdjęcia
			for (int i = 0; i < result.Rows; i++)
			{
				for (int j = 0; j < result.Cols; j++)
				{
					maxIntensity = 0;

					//Iteruj po wszystkich zdjęciach
					for (int z = 0; z < _photos.Count; z++)
					{
						intensity = 0;

						//Sprawdź intensywność całego obszaru wokół piksela
						//Jeśli maskSize = 0 sprawdzany jest tylko dany piksel
						for (int x = -maskSize; x <= maskSize; x++)
						{
							if (i + x < 0 || i + x >= result.Rows)
								continue;
							for (int y = -maskSize; y <= maskSize; y++)
							{
								if (j + y < 0 || j + y >= result.Cols)
									continue;
								intensity += _photos[z].MatrixAfterLaplace!.At<byte>(i + x, j + y);
							}
						}

						//Jeśli intensywność na danym zdjęciu jest największa, użyj na zdjęciu docelowym tego piksela
						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							result.At<Vec3b>(i, j) = _photos[z].Matrix.At<Vec3b>(i, j);
						}
					}

				}
			}
		}
	}
}
