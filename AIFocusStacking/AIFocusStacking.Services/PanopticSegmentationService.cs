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
			List<List<int>> intensities = new();
			GetIntensities(photos, intensities);
			ChooseBestMasks(photos, intensities);
		}

		private static void ChooseBestMasks(List<Photo> photos, List<List<int>> intensities)
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

		private static void GetIntensities(List<Photo> photos, List<List<int>> intensities)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Mat imageToMask = photos[i].Matrix.Clone();

				List<List<Point>> segments = new();
				List<Rect> boxes = new();
				JArray segmentsJson = JArray.Parse(File.ReadAllText($"panoptic_masks{photos[i].Path.Split('\\').Last()}.json"));
				JArray segmentsInfo = JArray.Parse(File.ReadAllText($"panoptic_classes_{photos[i].Path.Split('\\').Last()}.json"));
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
				for (int j = 1; j <= segmentsInfo.Count; j++)
				{
					List<Point> currentSegment = new();
					for (int k = 0; k < segmentsJson.Count; k++)
					{
						for(int l = 0; l < segmentsJson[0].Count(); l++)
						{							
							if ((int)segmentsJson[k]![l]! == j)
							{
								currentSegment.Add(new Point(k, l));
							}
						}
					}
					boxes.Add(Cv2.BoundingRect(currentSegment));
					segments.Add(currentSegment);
				}

				List<int> currentIntensities = new();

				for (int j = 0; j < segments.Count; j++)
				{
					int maskIntensity = 0;
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					for (int k = 0; k < segments[j].Count; k++)
					{
						Mask.At<Vec3b>(segments[j][k].X, segments[j][k].Y) = new Vec3b(255,255,255);
						
					}
					for (int k = boxes[j].Left + 1; k < boxes[j].Right; k++)
					{
						for (int l = boxes[j].Top + 1; l < boxes[j].Bottom; l++)
						{
							if (Mask.At<byte>(k, l) == 255)
							{
								maskIntensity += photos[i].MatrixAfterLaplace!.At<byte>(k, l);
							}
						}
					}
					currentIntensities.Add(maskIntensity);
				}

				intensities.Add(currentIntensities);
			}
		}

	}
}
