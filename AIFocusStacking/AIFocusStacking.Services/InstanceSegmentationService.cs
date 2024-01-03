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
			GetIntensities(photos);
			ChooseBestMasks(photos);
		}

		private static void ChooseBestMasks(List<Photo> photos)
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

				JArray masksJson = JArray.Parse(File.ReadAllText($"masks_{photos[index].Path.Split('\\').Last()}.json"));
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

		private static void GetIntensities(List<Photo> photos)
		{
			for (int i = 0; i < photos.Count; i++)
			{
				Photo photo = photos[i];
				Mat imageToMask = photo.Matrix.Clone();

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
