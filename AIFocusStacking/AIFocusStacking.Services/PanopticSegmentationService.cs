using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.Globalization;

namespace AIFocusStacking.Services
{
	//Serwis odpowiedzialny za panoptyczną segmentacje
	public class PanopticSegmentationService : IPanopticSegmentationService
	{
		//Serwis uruchamiający komendy w konsoli
		protected readonly IConsoleCommandsService _commandsService;

		//Serwis odpowiedzialny za dobieranie wykrytych obiektów w pary
		protected readonly IFeatureMatchingService _featureMatchingService;
		public PanopticSegmentationService(IConsoleCommandsService commandsService, IFeatureMatchingService featureMatchingService)
		{
			_commandsService = commandsService; 
			_featureMatchingService = featureMatchingService;
		}

		//Uruchom panoptyczną segmentacje
		public void RunPanopticSegmentation(List<Photo> photos, string confidence)
		{
			//Uruchom Detectron2
			_commandsService.RunModel("3", confidence);

			//Pobierz intensywności danych obiektów 
			GetIntensities(photos);

			//Dopasuj te same obiekty ze sobą i dodaj je do kolekcji
			//Każdy rekord kolekcji zawiera listę obiektów, której długość odpowiada liczbie zdjęć
			List<List<DetectedObject>> matchedObjects = MatchObjects(photos);

			//Wybierz obiekty z największą intensywnością pikseli
			ChooseBestMasks(photos, matchedObjects);
		}

		//Funkcja dopasowująca te same obiekty ze sobą
		private List<List<DetectedObject>> MatchObjects(List<Photo> photos)
		{
			//Kolekcja obiektów, które już zostały dopasowane
			List<DetectedObject> matchedObjects = new();

			//Lista kolekcji tych samych obiektów
			List<List<DetectedObject>> sameObjects = new();

			//Graniczna liczba dopasowań po któej stwoerdza się, że obiekty są te same
			int matchesCutOff = 0;

			//Iteruj przez wszystkie zdjęcia
			for (int i = 0; i < photos.Count; i++)
			{
				//Aktualne zdjęcie
				Photo photo = photos[i];

				//Iteruj przez wszystkie wykryte obiekty danego zdjęcia
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					if (!matchedObjects.Contains(photo.DetectedObjects[j]))
					{
						//Jeśli obiekt jeszcze nie został dopasowany
						matchedObjects.Add(photo.DetectedObjects[j]);

						//Inicjalizuj liste aktualnie dopasowanych obiektów
						List<DetectedObject> currentMatchedObjects = new()
						{
							photo.DetectedObjects[j]
						};

						//Stwórz podmacierz zawierającą dany obiekt
						Mat objectMatrix = photo.Matrix.SubMat(photo.DetectedObjects[j].Box);

						//Iteruj przez wszystkie zdjęcia
						for (int k = 0; k < photos.Count; k++)
						{
							//Jeśli zdjęcie nie jest aktualnym zdjęciem
							if (k != i)
							{
								int mostMatches = 0;
								int index = 0;

								//Iteruj przez wszystkie wykryte obiekty danego zdjęcia
								for (int l = 0; l < photos[k].DetectedObjects!.Count; l++)
								{
									//Jeśli obiekt ma taką samą klasę jak obiekt do którego go dopasowywujemy oraz nie został jeszcze dopasowany
									if (photos[k].DetectedObjects![l].Class == photo.DetectedObjects[j].Class && !matchedObjects.Contains(photos[k].DetectedObjects![l]))
									{
										//Stwórz podmacierz zawierającą dany obiekt
										Mat checkedObjectMatrix = photos[k].Matrix.SubMat(photos[k].DetectedObjects![l].Box);

										//Pobierz liczbę dopasowanych punktów kluczowych
										int amountOfMatches = _featureMatchingService.GetAmountOfMatches(objectMatrix, checkedObjectMatrix);

										/* Jeśli liczba dopasowanych punktów kluczowych jest największa spośród obiektów na danym zdjęciu,
										wybierz ten obiekt jako dopasowanie */
										if (amountOfMatches >= mostMatches)
										{
											mostMatches = amountOfMatches;
											index = l;
										}
									}
								}

								//Jeśli liczba dopasowanych punktów kluczowych jest większa niż wartość graniczna oraz obiekt nie został jeszcze dopasowany
								if (mostMatches >= matchesCutOff && !matchedObjects.Contains(photos[k].DetectedObjects![index]))
								{
									//Dodaj obiekt do obiektów już dopasowanych i aktualnie dopasowywanych obiektów
									matchedObjects.Add(photos[k].DetectedObjects![index]);
									currentMatchedObjects.Add(photos[k].DetectedObjects![index]);
								}
								//Jeśli nie
								else
								{
									//Stwórz nowy obiekt, który jest kopią aktualnego obiektu, ale jesgo intensywność jest równa 0
									DetectedObject objectToAdd = new DetectedObject(photo.DetectedObjects[j].Mask, photo.DetectedObjects[j].Box, photo.DetectedObjects[j].Class) { Intensity = 0 };

									//Dodaj do aktualnego zdjęcia stworzony obiekt
									photos[k].DetectedObjects!.Add(objectToAdd);

									//Dodaj stworzony obiekt do obiektów już dopasowanych i aktualnie dopasowywanych obiektów
									matchedObjects.Add(objectToAdd);
									currentMatchedObjects.Add(objectToAdd);
								}
							}
						}
						//Dodaj kolekcję dopasowanych obiektów do listy
						sameObjects.Add(currentMatchedObjects);
					}
				}

			}
			//Zwróć listę kolekcji dopasowanych obiektów
			return sameObjects;
		}

		/* Funkcja wybierająca obiekty z największą intensywnością pikseli
		Funkcja zamienia obszary na zdjęciach po filtrze Laplace'a na czarny lub biał kolor zależnie od tego,
		czy zawierają obiekt z największą intensywnością pikseli */
		private static void ChooseBestMasks(List<Photo> photos, List<List<DetectedObject>> matchedObjects)
		{
			//Iteruj przez wszystkie kolekcje dobranych obiektów
			for (int i = 0; i < matchedObjects.Count; i++)
			{
				//Największa intensywność pikseli
				int maxMaskIntensity = 0;
				//Obiekt z największą intensywnością pikseli
				DetectedObject bestObject = new(new List<Point>(), new Rect(), 0);

				//Wybierz obiekt z największą intensywnością pikseli
				for (int j = 0; j < photos.Count; j++)
				{
					if (matchedObjects[i][j].Intensity > maxMaskIntensity) { maxMaskIntensity = (int)matchedObjects[i][j].Intensity!; bestObject = matchedObjects[i][j]; }
				}

				//Zdjęcie zawierające obiekt z największą intensywnością pikseli
				Photo photoWithBestObject = new(new Mat(), "");

				//Iteruj przez wszystkie zdjęcia
				for (int j = 0; j < photos.Count; j++)
				{
					//Jeśli zdjęcie zawiera obiekt z największą intensywnością pikseli
					if (photos[j].DetectedObjects!.Contains(bestObject))
					{
						//Zamień piksele należące do najlepszego obiektu na zdjęciu, po filtrze Laplace'a, które go zawiera na wartość 255
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 255;
						}

						//Zapisz zdjęcie zawierające obiekt z największą intensywnością pikseli
						photoWithBestObject = photos[j];
						break;
					}
				}

				//Iteruj przez wszystkie zdjęcia
				for (int j = 0; j < photos.Count; j++)
				{
					//Jeśli zdjęcie nie zawiera obiektu z największą intensywnością pikseli
					if (photos[j] != photoWithBestObject)
					{
						//Iteruj przez wszystkie wykryte obiekty na danym zdjęciu
						for (int k = 0; k < photos[j].DetectedObjects!.Count; k++)
						{
							//Jeśli dany obiekt znajduje się w aktualnej kolekcji dopasowanych obiektów 
							if (matchedObjects[i].Contains(photos[j].DetectedObjects![k]))
							{
								//Zamień piksele należące do aktualnego obiektu na zdjęciu, po filtrze Laplace'a, które go zawiera na wartość 0
								//a na zdjęciu zawierającym obiekt z największą intensywnością na wartość 255
								for (int l = 0; l < photos[j].DetectedObjects![k].Mask.Count; l++)
								{
									photos[j].MatrixAfterLaplace!.At<byte>(photos[j].DetectedObjects![k].Mask[l].Y, photos[j].DetectedObjects![k].Mask[l].X) = 0;
									photoWithBestObject.MatrixAfterLaplace!.At<byte>(photos[j].DetectedObjects![k].Mask[l].Y, photos[j].DetectedObjects![k].Mask[l].X) = 255;
								}
								break;
							}
						}

						//Zamień piksele należące do najlepszego obiektu na aktualnym zdjęciu, po filtrze Laplace'a, na wartość 0
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 0;
						}
					}				
				}
				//Zapisz zdjęcia po filtrze Laplace'a
				for (int j = 0; j < photos.Count; j++)
				{
					photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}
			}
		}

		//Funkcja pobierająca intensywności danych obiektów 
		private static void GetIntensities(List<Photo> photos)
		{
			//Iteruj po wszystkich zdjęciach
			for (int i = 0; i < photos.Count; i++)
			{
				//Aktualne zdjęcie
				Photo photo = photos[i];
				Mat imageToMask = photos[i].Matrix.Clone();

				//Kolekcja wszystkich obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray masksJson = JArray.Parse(File.ReadAllText($"panoptic_masks_{photo.Path.Split('\\').Last()}.json"));

				//Kolekcja wszystkich klas obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray classesJson = JArray.Parse(File.ReadAllText($"panoptic_classes_{photo.Path.Split('\\').Last()}.json"));

				//Zamień wszystkie punkty o wartości 0 na wartość wcześniejszego piksela
				for (int j = 0; j < masksJson.Count; j++)
				{
					for (int k = 0; k < masksJson[0].Count(); k++)
					{
						if ((int)masksJson[j]![k]! == 0)
						{
							if (k == 0)
							{
								if (j == 0)
								{
									masksJson[j][k] = 1;
								}
								else
								{
									masksJson[j][k] = masksJson[j - 1][masksJson[0].Count() - 1];
								}
							}
							else
							{
								masksJson[j][k] = masksJson[j][k - 1];
							}
						}
					}
				}

				//Iteruj po klasach
				for (int j = 1; j <= classesJson.Count; j++)
				{
					List<Point> currentMask = new();

					//Iteruj po punktach
					for (int k = 0; k < masksJson.Count; k++)
					{
						for(int l = 0; l < masksJson[0].Count(); l++)
						{
							//Jeśli klasa punktu jest równa aktualnej klasie 
							if ((int)masksJson[k]![l]! == j)
							{
								//Dodaj punkt do aktualnej maski obiektu
								currentMask.Add(new Point(l, k));
							}
						}
					}

					//Stwórz ramkę ograniczająca danego obiektu
					Rect box = Cv2.BoundingRect(currentMask);

					//Stwórz kolekcję wykrytych obiektów jeśli nie istnieje
					if (photo.DetectedObjects == null)
					{
						photo.DetectedObjects = new();
					}

					//Dodaj nowy obiekt do kolekcji
					photo.DetectedObjects.Add(new DetectedObject(currentMask, box, (int)classesJson[j-1]));
				}

				//Iteruj po wykrytych obiektach
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					//Maska i ramka aktualnego obiektu
					List<Point> mask = photo.DetectedObjects[j].Mask;
					Rect box = photo.DetectedObjects[j].Box;

					

					//Stwórz maskę i zamień na niej wszystkie piksele znajdujace się w aktualnym obiekcie na kolor biały
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					for (int k = 0; k < mask.Count; k++)
					{
						Mask.At<Vec3b>(mask[k].Y,mask[k].X) = new Vec3b(255,255,255);
						
					}

					int maskIntensity = 0;
					int numberOfPixels = 0;

					//Iteruj po wszystkich pikselach w zakresie ramki ograniczającej aktualnego obiektu
					for (int k = box.Top + 1; k < box.Bottom; k++)
					{
						for (int l = box.Left + 1; l < box.Right; l++)
						{
							//Jeśli piksel w masce ma intensywność 255
							if (Mask.At<byte>(k, l) == 255)
							{
								//Dodaj intensywność piksela z aktualnego zdjęcia po filtrze Laplace'a
								maskIntensity += photo.MatrixAfterLaplace!.At<byte>(k, l);
								numberOfPixels++;
							}
						}
					}
					//Ustaw intensywność aktualnego obiektu na średnią intensywność pikseli
					photo.DetectedObjects[j].Intensity = maskIntensity/numberOfPixels;
				}

			}
		}

	}
}
