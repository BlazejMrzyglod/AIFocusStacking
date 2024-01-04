using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	public class InstanceSegmentationService : IInstanceSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		protected readonly IFeatureMatchingService _featureMatchingService;
		public InstanceSegmentationService(IConsoleCommandsService commandsService, IFeatureMatchingService featureMatchingService)
		{
			_commandsService = commandsService;
			_featureMatchingService = featureMatchingService;
		}
		public void RunInstanceSegmentation(List<Photo> photos)
		{
			_commandsService.RunModel("2");
			GetObjects(photos);
			GetIntensities(photos);
			List<List<DetectedObject>> matchedObjects = MatchObjects(photos);
			ChooseBestMasks(photos, matchedObjects);
		}

		private void ChooseBestMasks(List<Photo> photos, List<List<DetectedObject>> matchedObjects)
		{
			for (int i = 0; i < matchedObjects.Count; i++)
			{
				int maxMaskIntensity = 0;
				DetectedObject bestObject = new(new List<Point>(), new Rect(), 0);
				for (int j = 0; j < photos.Count; j++)
				{
					if (matchedObjects[i][j].Intensity > maxMaskIntensity) { maxMaskIntensity = (int)matchedObjects[i][j].Intensity; bestObject = matchedObjects[i][j]; }
				}

				for (int j = 0; j < photos.Count; j++)
				{
					if (photos[j].DetectedObjects.Contains(bestObject))
					{
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { bestObject.Mask }, -1, new Scalar(255, 255, 255), -1);
					}
					else
					{
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() {bestObject.Mask }, -1, new Scalar(0, 0, 0), -1);
					}
					photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}

			}
		}

		private List<List<DetectedObject>> MatchObjects(List<Photo> photos)
		{
			List<DetectedObject> matchedObjects = new();
			List<List<DetectedObject>> sameObjects = new();
			int matchesCutOff = 0;
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					if (!matchedObjects.Contains(photo.DetectedObjects[j]))
					{
						matchedObjects.Add(photo.DetectedObjects[j]);
						List<DetectedObject> currentMatchedObjects = new();
						currentMatchedObjects.Add(photo.DetectedObjects[j]);
						Mat objectMatrix = photo.Matrix.SubMat(photo.DetectedObjects[j].Box);
						for (int k = 0; k < photos.Count; k++)
						{
							if (k != i)
							{
								int mostMatches = 0;
								int index = 0;
								for (int l = 0; l < photos[k].DetectedObjects!.Count; l++)
								{
									if (photos[k].DetectedObjects![l].Class == photo.DetectedObjects[j].Class && !matchedObjects.Contains(photos[k].DetectedObjects![l]))
									{
										Mat checkedObjectMatrix = photos[k].Matrix.SubMat(photos[k].DetectedObjects![l].Box);
										int amountOfMatches = _featureMatchingService.GetAmountOfMatches(objectMatrix, checkedObjectMatrix);

										if (amountOfMatches > mostMatches)
										{
											mostMatches = amountOfMatches;
											index = l;
										}
									}
								}

								if (mostMatches >= matchesCutOff && !matchedObjects.Contains(photos[k].DetectedObjects![index]))
								{
									matchedObjects.Add(photos[k].DetectedObjects![index]);
									currentMatchedObjects.Add(photos[k].DetectedObjects![index]);
								}
								else
								{
									DetectedObject objectToAdd = new DetectedObject(photo.DetectedObjects[j].Mask, photo.DetectedObjects[j].Box, photo.DetectedObjects[j].Class) { Intensity = 0 };
									photos[k].DetectedObjects!.Add(objectToAdd);
									matchedObjects.Add(objectToAdd);
									currentMatchedObjects.Add(objectToAdd);
								}
							}
						}
						sameObjects.Add(currentMatchedObjects);
					}
				}

			}
			return sameObjects;
		}

		private static void GetObjects(List<Photo> photos)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				JArray contoursJson = JArray.Parse(File.ReadAllText($"contours_{photo.Path.Split('\\').Last()}.json"));
				JArray classesJson = JArray.Parse(File.ReadAllText($"classes_{photo.Path.Split('\\').Last()}.json"));

				if (photo.DetectedObjects == null)
				{
					photo.DetectedObjects = new();
				}

				for (int j = 0; j < contoursJson.Count; j++)
				{
					JToken? contour = contoursJson[j];
					JToken? _class = classesJson[j];
					List<Point> currentContour = new();

					foreach (var points in contour)
					{
						currentContour.Add(new Point((int)points[0]![0]!, (int)points[0]![1]!));
					}

					Rect currentBox = Cv2.BoundingRect(currentContour);
					photo.DetectedObjects!.Add(new DetectedObject(currentContour, currentBox, (int)_class));
				}
			}
		}
		private static void GetIntensities(List<Photo> photos)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				Mat imageToMask = photo.Matrix.Clone();

				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					DetectedObject detectedObject = photo.DetectedObjects[j];
					int maskIntensity = 0;
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));

					Cv2.DrawContours(Mask, new List<List<Point>>() { detectedObject.Mask }, -1, new Scalar(255, 255, 255), -1);

					for (int k = detectedObject.Box.Top; k <= detectedObject.Box.Bottom; k++)
					{
						for (int l = detectedObject.Box.Left; l <= detectedObject.Box.Right; l++)
						{
							if (Mask.At<byte>(k, l) == 255)
							{
								maskIntensity += photo.MatrixAfterLaplace!.At<byte>(k, l);
							}
						}
					}
					detectedObject.Intensity = maskIntensity;
				}
			}
		}
	}
}
