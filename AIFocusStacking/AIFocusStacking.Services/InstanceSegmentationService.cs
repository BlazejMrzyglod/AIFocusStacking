using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	public class InstanceSegmentationService : IInstanceSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		public InstanceSegmentationService(IConsoleCommandsService commandsService) { _commandsService = commandsService; }
		public void RunInstanceSegmentation(List<Photo> photos)
		{
			_commandsService.RunModel("2");
			List<List<int>> intensities = new();
			GetIntensities(photos, intensities);
			ChooseBestMasks(photos, intensities);
		}

		private static void ChooseBestMasks(List<Photo> photos, List<List<int>> intensities)
		{
			for (int i = 0; i < intensities.First().Count; i++)
			{
				int maxMaskIntensity = 0;
				int index = 0;
				for (int j = 0; j < photos.Count; j++)
				{
					if (intensities[j][i] > maxMaskIntensity) { maxMaskIntensity = intensities[j][i]; index = j; }
				}
				List<List<Point>> contours = new();
				List<Point> currentMask = new();

				JArray masksJson = JArray.Parse(File.ReadAllText($"masks{photos[index].Path!.Split('\\').Last()}.json"));
				foreach (var points in masksJson[i])
				{
					currentMask.Add(new Point((int)points[0]![0]!, (int)points[0]![1]!));
				}
				contours.Add(currentMask);
				for (int j = 0; j < photos.Count; j++)
				{
					if (j == index)
					{
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, contours, -1, new Scalar(255, 255, 255), -1);
					}
					else
					{
						Cv2.DrawContours(photos[j].MatrixAfterLaplace!, contours, -1, new Scalar(0, 0, 0), -1);
					}
					photos[j].MatrixAfterLaplace!.SaveImage($"laplace{j}.jpg");
				}

			}
		}

		private static void GetIntensities(List<Photo> photos, List<List<int>> intensities)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Mat imageToMask = photos[i].Matrix!.Clone();

				List<List<Point>> contours = new();
				List<Rect> boxes = new();
				JArray masksJson = JArray.Parse(File.ReadAllText($"masks{photos[i].Path!.Split('\\').Last()}.json"));
				foreach (var mask in masksJson)
				{
					List<Point> currentMask = new();

					foreach (var points in mask)
					{
						currentMask.Add(new Point((int)points[0]![0]!, (int)points[0]![1]!));
					}
					boxes.Add(Cv2.BoundingRect(currentMask));
					contours.Add(currentMask);
				}

				List<int> currentIntensities = new();

				for (int j = 0; j < contours.Count; j++)
				{
					int maskIntensity = 0;
					Mat Mask = new(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					Cv2.DrawContours(Mask, contours.GetRange(j, 1), -1, new Scalar(255, 255, 255), -1);
					for (int k = boxes[j].Top; k <= boxes[j].Bottom; k++)
					{
						for (int l = boxes[j].Left; l <= boxes[j].Right; l++)
						{
							if (Mask.At<byte>(k, l) == 255)
							{
								maskIntensity += photos[j].MatrixAfterLaplace!.At<byte>(k, l);
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
