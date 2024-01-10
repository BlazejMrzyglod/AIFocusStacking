﻿using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.Linq;

namespace AIFocusStacking.Services
{
	//Serwis odpowiedzialny za segmentacje instancji
	public class InstanceSegmentationService : IInstanceSegmentationService
	{
		//Serwis uruchamiający komendy w konsoli
		protected readonly IConsoleCommandsService _commandsService;

		//Serwis odpowiedzialny za dobieranie wykrytych obiektów w pary
		protected readonly IFeatureMatchingService _featureMatchingService;
		public InstanceSegmentationService(IConsoleCommandsService commandsService, IFeatureMatchingService featureMatchingService)
		{
			_commandsService = commandsService;
			_featureMatchingService = featureMatchingService;
		}

		//Uruchom segmentacje instancji
		public void RunInstanceSegmentation(List<Photo> photos, string confidence)
		{
			//Uruchom Detectron2
			_commandsService.RunModel("2", confidence);

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

		/* Funkcja wybierająca obiekty z największą intensywnością pikseli
		Funkcja zamienia obszary na zdjęciach po filtrze Laplace'a na czarny lub biał kolor zależnie od tego,
		czy zawierają obiekt z największą intensywnością pikseli */
		private void ChooseBestMasks(List<Photo> photos, List<List<DetectedObject>> matchedObjects)
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
						//Narysuj wypełniony biały kontur danego obiektu na zdjęciu po filtrze Laplace'a
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { bestObject.Mask }, -1, new Scalar(255, 255, 255), -1);

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
								//Narysuj wypełniony czarny kontur danego obiektu na zdjęciu po filtrze Laplace'a, które nie zawieraja obiektu z największą intensywnością pikseli
								Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { photos[j].DetectedObjects![k].Mask }, -1, new Scalar(0, 0, 0), -1);

								//Narysuj wypełniony biały kontur danego obiektu na zdjęciu po filtrze Laplace'a, które zawieraja obiekt z największą intensywnością pikseli
								Cv2.DrawContours(photoWithBestObject.MatrixAfterLaplace!, new List<List<Point>>() { photos[j].DetectedObjects![k].Mask }, -1, new Scalar(255, 255, 255), -1);
								break;
							}
						}

						//Narysuj wypełniony czarny kontur obiektu z największą intensywnością pikseli na zdjęciu po filtrze Laplace'a, które go nie zawiera
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { bestObject.Mask }, -1, new Scalar(0, 0, 0), -1);
					}				
				}
				//Zapisz zdjęcia po filtrze Laplace'a
				for (int j = 0; j < photos.Count; j++)
				{
					photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}

			}
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
					//Jeśli obiekt jeszcze nie został dopasowany
					if (!matchedObjects.Contains(photo.DetectedObjects[j]))
					{
						//Dodaj obiekt do dopasowanych
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

		//Funkcja dodająca wykryte obiekty do zdjęć
		private static void GetObjects(List<Photo> photos)
		{
			//Iteruj przez wszystkie zdjęcia
			for (int i = 0; i < photos.Count; i++)
			{
				//Aktualne zdjęcie
				Photo photo = photos[i];

				//Kolekcja wszystkich konturów obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray contoursJson = JArray.Parse(File.ReadAllText($"contours_{photo.Path.Split('\\').Last()}.json"));

				//Kolekcja wszystkich klas obiektów wykrytych na zdjęciu pobrana z pliku json
				JArray classesJson = JArray.Parse(File.ReadAllText($"classes_{photo.Path.Split('\\').Last()}.json"));

				//Stwórz kolekcję wykrytych obiektów jeśli nie istnieje
				if (photo.DetectedObjects == null)
				{
					photo.DetectedObjects = new();
				}

				//Iteruj przez wszystkie kontury
				for (int j = 0; j < contoursJson.Count; j++)
				{
					//Aktualny kontur
					JToken? contour = contoursJson[j];

					//Klasa aktualnego obiektu
					JToken? _class = classesJson[j];

					//Pobierz wszystkie punkty konturu
					List<Point> currentContour = new();
					foreach (var points in contour)
					{
						currentContour.Add(new Point((int)points[0]![0]!, (int)points[0]![1]!));
					}

					//Stwórz ramkę ograniczająca danego obiektu
					Rect currentBox = Cv2.BoundingRect(currentContour);

					//Dodaj nowy obiekt do kolekcji
					photo.DetectedObjects!.Add(new DetectedObject(currentContour, currentBox, (int)_class));
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
				Mat imageToMask = photo.Matrix.Clone();

				//Iteruj po wykrytych obiektach
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					//Aktualny obiekt
					DetectedObject detectedObject = photo.DetectedObjects[j];
					int maskIntensity = 0;
					int numberOfPixels = 0;

					//Stwórz maskę i narysuj na niej wypełniony, biały kontur aktualnego obiektu
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					Cv2.DrawContours(Mask, new List<List<Point>>() { detectedObject.Mask }, -1, new Scalar(255, 255, 255), -1);

					//Iteruj po wszystkich pikselach w zakresie ramki ograniczającej aktualnego obiektu
					for (int k = detectedObject.Box.Top; k <= detectedObject.Box.Bottom; k++)
					{
						for (int l = detectedObject.Box.Left; l <= detectedObject.Box.Right; l++)
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
					detectedObject.Intensity = maskIntensity/numberOfPixels;
				}
			}
		}
	}
}
