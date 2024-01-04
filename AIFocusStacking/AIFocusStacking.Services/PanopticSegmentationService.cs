using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	public class PanopticSegmentationService : IPanopticSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		protected readonly IFeatureMatchingService _featureMatchingService;
		public PanopticSegmentationService(IConsoleCommandsService commandsService, IFeatureMatchingService featureMatchingService)
		{
			_commandsService = commandsService; 
			_featureMatchingService = featureMatchingService;
		}
		public void RunPanopticSegmentation(List<Photo> photos)
		{
			_commandsService.RunModel("3");
			GetIntensities(photos);
			List<List<DetectedObject>> matchedObjects = MatchObjects(photos);
			ChooseBestMasks(photos, matchedObjects);
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
						List<DetectedObject> currentMatchedObjects = new()
						{
							photo.DetectedObjects[j]
						};
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

										if (amountOfMatches >= mostMatches)
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
		private static void ChooseBestMasks(List<Photo> photos, List<List<DetectedObject>> matchedObjects)
		{
			for (int i = 0; i < matchedObjects.Count; i++)
			{
				int maxMaskIntensity = 0;
				DetectedObject bestObject = new(new List<Point>(), new Rect(), 0);
				for (int j = 0; j < photos.Count; j++)
				{
					if (matchedObjects[i][j].Intensity > maxMaskIntensity) { maxMaskIntensity = (int)matchedObjects[i][j].Intensity!; bestObject = matchedObjects[i][j]; }
				}
				Photo photoWithBestObject = new(new Mat(), "");
				for (int j = 0; j < photos.Count; j++)
				{
					if (photos[j].DetectedObjects!.Contains(bestObject))
					{
						//Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { bestObject.Mask }, -1, new Scalar(255, 255, 255), -1);
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 255;
						}
						photoWithBestObject = photos[j];
						break;
					}
				}
				for (int j = 0; j < photos.Count; j++)
				{
					if (photos[j] != photoWithBestObject)
					{
						for (int k = 0; k < photos[j].DetectedObjects!.Count; k++)
						{
							if (matchedObjects[i].Contains(photos[j].DetectedObjects![k]))
							{
								//Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { photos[j].DetectedObjects![k].Mask }, -1, new Scalar(0, 0, 0), -1);
								for (int l = 0; l < photos[j].DetectedObjects![k].Mask.Count; l++)
								{
									photos[j].MatrixAfterLaplace!.At<byte>(photos[j].DetectedObjects![k].Mask[l].Y, photos[j].DetectedObjects![k].Mask[l].X) = 0;
									photoWithBestObject.MatrixAfterLaplace!.At<byte>(photos[j].DetectedObjects![k].Mask[l].Y, photos[j].DetectedObjects![k].Mask[l].X) = 255;
								}
								//Cv2.DrawContours(photoWithBestObject.MatrixAfterLaplace!, new List<List<Point>>() { photos[j].DetectedObjects![k].Mask }, -1, new Scalar(255, 255, 255), -1);
								break;
							}
						}
						for (int k = 0; k < bestObject.Mask.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(bestObject.Mask[k].Y, bestObject.Mask[k].X) = 0;
						}
						//Cv2.DrawContours(photos[j].MatrixAfterLaplace!, new List<List<Point>>() { bestObject.Mask }, -1, new Scalar(0, 0, 0), -1);
					}				
				}
				for (int j = 0; j < photos.Count; j++)
				{
					photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}

				}
		}

		private static void GetIntensities(List<Photo> photos)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				Mat imageToMask = photos[i].Matrix.Clone();

				JArray masksJson = JArray.Parse(File.ReadAllText($"panoptic_masks{photo.Path.Split('\\').Last()}.json"));
				JArray classesJson = JArray.Parse(File.ReadAllText($"panoptic_classes_{photo.Path.Split('\\').Last()}.json"));

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

				for (int j = 0; j <= classesJson.Count; j++)
				{
					List<Point> currentMask = new();
					for (int k = 0; k < masksJson.Count; k++)
					{
						for(int l = 0; l < masksJson[0].Count(); l++)
						{							
							if ((int)masksJson[k]![l]! == j)
							{
								currentMask.Add(new Point(k, l));
							}
						}
					}

					Rect box = Cv2.BoundingRect(currentMask);

					if (photo.DetectedObjects == null)
					{
						photo.DetectedObjects = new();
					}

					photo.DetectedObjects.Add(new DetectedObject(currentMask, box, (int)classesJson[j]));
				}

				for (int j = 0; j < photo.DetectedObjects!.Count; j++)
				{
					List<Point> mask = photo.DetectedObjects[j].Mask;
					Rect box = photo.DetectedObjects[j].Box;

					int maskIntensity = 0;
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));

					for (int k = 0; k < mask.Count; k++)
					{
						Mask.At<Vec3b>(mask[k].X,mask[k].Y) = new Vec3b(255,255,255);
						
					}
					for (int k = box.Left + 1; k < box.Right; k++)
					{
						for (int l = box.Top + 1; l < box.Bottom; l++)
						{
							if (Mask.At<byte>(k, l) == 255)
							{
								maskIntensity += photo.MatrixAfterLaplace!.At<byte>(k, l);
							}
						}
					}

					photo.DetectedObjects[j].Intensity = maskIntensity;
				}

			}
		}

	}
}
