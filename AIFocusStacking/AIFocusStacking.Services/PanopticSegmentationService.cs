using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	//Serwis odpowiedzialny za panoptyczną segmentacje
	public class PanopticSegmentationService : IPanopticSegmentationService
	{
		//Serwis uruchamiający komendy w konsoli
		protected readonly IConsoleCommandsService _commandsService;

		//Serwis odpowiedzialny za dobieranie wykrytych obiektów w pary
		protected readonly IFeatureMatchingService _featureMatchingService;

		//Repozytprium plikó json
		protected readonly IRepositoryService<JArray> _jsonRepositoryService;
		public PanopticSegmentationService(IConsoleCommandsService commandsService, IFeatureMatchingService featureMatchingService, IRepositoryService<JArray> jsonRepositoryService)
		{
			_commandsService = commandsService;
			_featureMatchingService = featureMatchingService;
			_jsonRepositoryService = jsonRepositoryService;
		}

		//Uruchom panoptyczną segmentacje
		public void RunPanopticSegmentation(List<Photo> photos, string confidence)
		{
			//Uruchom Detectron2
			_ = _commandsService.RunModel("3", confidence);

			//Dodaj wykryte obiekty do zdjęć
			GetObjects(photos);

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

			//Iteruj przez wszystkie zdjęcia
			for (int i = 0; i < photos.Count; i++)
			{
				//Aktualne zdjęcie
				Photo photo = photos[i];

				//Iteruj przez wszystkie wykryte obiekty danego zdjęcia
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					DetectedObject detectedObject = photo.DetectedObjects[j];
					if (!matchedObjects.Contains(detectedObject))
					{
						//Jeśli obiekt jeszcze nie został dopasowany
						matchedObjects.Add(detectedObject);

						//Inicjalizuj liste aktualnie dopasowanych obiektów
						List<DetectedObject> currentMatchedObjects = new()
						{
							detectedObject
						};

						//Stwórz podmacierz zawierającą dany obiekt
						Mat objectMatrix = photo.Matrix.SubMat(detectedObject.Box);

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
									DetectedObject objectToCheck = photos[k].DetectedObjects![l];
									//Jeśli obiekt ma taką samą klasę jak obiekt do którego go dopasowywujemy oraz nie został jeszcze dopasowany
									if (objectToCheck.Class == detectedObject.Class && !matchedObjects.Contains(objectToCheck))
									{
										//Stwórz podmacierz zawierającą dany obiekt
										Mat checkedObjectMatrix = photos[k].Matrix.SubMat(objectToCheck.Box);

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

								DetectedObject bestObject = photos[k].DetectedObjects![index];

								//Jeśli obiekt nie został jeszcze dopasowany
								if (!matchedObjects.Contains(bestObject))
								{
									//Dodaj obiekt do obiektów już dopasowanych i aktualnie dopasowywanych obiektów
									matchedObjects.Add(bestObject);
									currentMatchedObjects.Add(bestObject);
								}
								//Jeśli nie
								else
								{
									//Stwórz nowy obiekt, który jest kopią aktualnego obiektu, ale jesgo intensywność jest równa 0
									DetectedObject objectToAdd = new(detectedObject.Mask, detectedObject.Box, detectedObject.Class) { Intensity = 0 };

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

				//Aktualna lista dopasowanych obiektów
				List<DetectedObject> currentList = matchedObjects[i];

				//Obiekt z największą intensywnością pikseli
				DetectedObject bestObject = new(new List<Point>(), new Rect(), 0);

				//Wybierz obiekt z największą intensywnością pikseli
				for (int j = 0; j < photos.Count; j++)
				{
					if (matchedObjects[i][j].Intensity > maxMaskIntensity)
					{
						maxMaskIntensity = (int)matchedObjects[i][j].Intensity!;
						bestObject = matchedObjects[i][j];
					}
				}

				//Zdjęcie zawierające obiekt z największą intensywnością pikseli
				Photo photoWithBestObject = new(new Mat(), "");

				//Iteruj przez wszystkie zdjęcia
				for (int j = 0; j < photos.Count; j++)
				{
					Photo photo = photos[j];

					//Jeśli zdjęcie zawiera obiekt z największą intensywnością pikseli
					if (photo.DetectedObjects!.Contains(bestObject))
					{
						//Zamień piksele należące do najlepszego obiektu na zdjęciu, po filtrze Laplace'a, które go zawiera na wartość 255
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photo.MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 255;
						}

						//Zapisz zdjęcie zawierające obiekt z największą intensywnością pikseli
						photoWithBestObject = photo;
						break;
					}
				}

				//Iteruj przez wszystkie zdjęcia
				for (int j = 0; j < photos.Count; j++)
				{
					Photo photo = photos[j];

					//Jeśli zdjęcie nie zawiera obiektu z największą intensywnością pikseli
					if (photo != photoWithBestObject)
					{
						//Iteruj przez wszystkie wykryte obiekty na danym zdjęciu
						for (int k = 0; k < photo.DetectedObjects!.Count; k++)
						{
							DetectedObject objectToCheck = photo.DetectedObjects[k];

							//Jeśli dany obiekt znajduje się w aktualnej kolekcji dopasowanych obiektów 
							if (currentList.Contains(objectToCheck))
							{
								//Zamień piksele należące do aktualnego obiektu na zdjęciu, po filtrze Laplace'a, które go zawiera na wartość 0
								//a na zdjęciu zawierającym obiekt z największą intensywnością na wartość 255
								for (int l = 0; l < objectToCheck.Mask.Count; l++)
								{
									photo.MatrixAfterLaplace!.At<byte>(objectToCheck.Mask[l].Y, objectToCheck.Mask[l].X) = 0;
									photoWithBestObject.MatrixAfterLaplace!.At<byte>(objectToCheck.Mask[l].Y, objectToCheck.Mask[l].X) = 255;
								}
								break;
							}
						}

						//Zamień piksele należące do najlepszego obiektu na aktualnym zdjęciu, po filtrze Laplace'a, na wartość 0
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photo.MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 0;
						}
					}
				}
				//Zapisz zdjęcia po filtrze Laplace'a
				for (int j = 0; j < photos.Count; j++)
				{
					_ = photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}
			}
		}

		//Funkcja pobierająca intensywności danych obiektów 
		private static void GetIntensities(List<Photo> photos)
		{
			//Iteruj po wszystkich zdjęciach
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				Mat imageToMask = photos[i].Matrix.Clone();


				//Iteruj po wykrytych obiektach
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					DetectedObject detectedObject = photo.DetectedObjects[j];
					//Maska i ramka aktualnego obiektu
					List<Point> detectedObjectMask = detectedObject.Mask;
					Rect box = detectedObject.Box;

					int maskIntensity = 0;
					int numberOfPixels = 0;

					//Stwórz maskę i zamień na niej wszystkie piksele znajdujace się w aktualnym obiekcie na kolor biały
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					for (int k = 0; k < detectedObjectMask.Count; k++)
					{
						Mask.At<Vec3b>(detectedObjectMask[k].Y, detectedObjectMask[k].X) = new Vec3b(255, 255, 255);
					}

					//Iteruj po wszystkich pikselach w zakresie ramki ograniczającej aktualnego obiektu
					for (int k = box.Top; k <= box.Bottom; k++)
					{
						for (int l = box.Left; l <= box.Right; l++)
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
					detectedObject.Intensity = maskIntensity / numberOfPixels;
				}

			}
		}

		private void GetObjects(List<Photo> photos)
		{
			//Iteruj po wszystkich zdjęciach
			for (int i = 0; i < photos.Count; i++)
			{
				//Aktualne zdjęcie
				Photo photo = photos[i];

				//Kolekcja wszystkich obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray masksJson = _jsonRepositoryService.GetSingle($"panoptic_masks_{photo.Name.Split('\\').Last()}.json");

				//Kolekcja wszystkich klas obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray classesJson = _jsonRepositoryService.GetSingle($"panoptic_classes_{photo.Name.Split('\\').Last()}.json");

				//Zamień wszystkie punkty o wartości 0 na wartość wcześniejszego piksela
				for (int j = 0; j < masksJson.Count; j++)
				{
					for (int k = 0; k < masksJson[j].Count(); k++)
					{
						if ((int)masksJson[j]![k]! == 0)
						{
							masksJson[j][k] = k == 0 ? j == 0 ? (JToken)1 : masksJson[j - 1][masksJson[j].Count() - 1] : masksJson[j][k - 1];
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
						for (int l = 0; l < masksJson[j].Count(); l++)
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
					photo.DetectedObjects ??= new();

					//Dodaj nowy obiekt do kolekcji
					photo.DetectedObjects.Add(new DetectedObject(currentMask, box, (int)classesJson[j - 1]));
				}
			}
		}
	}
}
