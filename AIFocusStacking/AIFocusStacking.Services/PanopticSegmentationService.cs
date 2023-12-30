using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	public class PanopticSegmentationService : IPanopticSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		public PanopticSegmentationService(IConsoleCommandsService commandsService) { _commandsService = commandsService; }
		public void RunPanopticSegmentation(List<Photo> photos)
		{
			_commandsService.RunModel("3");
			GetIntensities(photos);
			ChooseBestMasks(photos);
		}

		private static void ChooseBestMasks(List<Photo> photos)
		{
			int segmentsAmount = intensities.First().Count;
			for (int i = 1; i < intensities.Count; i++)
			{
				if (intensities[i].Count != segmentsAmount)
				{
					throw new Exception("Różne liczby segmentów");
				}
			}
			for (int i = 0; i < segmentsAmount; i++)
			{
				int maxMaskIntensity = 0;
				int index = 0;
				for (int j = 0; j < photos.Count; j++)
				{
					if (intensities[j][i] > maxMaskIntensity) { maxMaskIntensity = intensities[j][i]; index = j; }
				}
				List<Point> segment = new();
				JArray segmentsJson = JArray.Parse(File.ReadAllText($"panoptic_masks_{photos[index].Path.Split('\\').Last()}.json"));

				for (int j = 0; j < segmentsJson.Count; j++)
				{
					for (int k = 0; k < segmentsJson[0].Count(); k++)
					{
						if ((int)segmentsJson[j]![k]! == 0)
						{
							if (k == 0)
							{
								if (j == 0)
								{
									segmentsJson[j][k] = 1;
								}
								else
								{
									segmentsJson[j][k] = segmentsJson[j - 1][segmentsJson[0].Count() - 1];
								}
							}
							else
							{
								segmentsJson[j][k] = segmentsJson[j][k - 1];
							}
						}
					}
				}

				for (int k = 0; k < segmentsJson.Count; k++)
				{
					for (int l = 0; l < segmentsJson[0].Count(); l++)
					{
						if ((int)segmentsJson[k]![l]! == i + 1)
						{
							segment.Add(new Point(k, l));
						}
					}
				}

				for (int j = 0; j < photos.Count; j++)
				{
					if (j == index)
					{
						for (int k = 0; k < segment.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(segment[k].X, segment[k].Y) = 255;
						}
					}
					else
					{
						for (int k = 0; k < segment.Count; k++)
						{
							photos[j].MatrixAfterLaplace!.At<byte>(segment[k].X, segment[k].Y) = 0;
						}
					}
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
