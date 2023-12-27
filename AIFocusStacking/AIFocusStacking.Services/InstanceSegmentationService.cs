using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	public class InstanceSegmentationService : IInstanceSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		public InstanceSegmentationService(IConsoleCommandsService commandsService) { _commandsService = commandsService; }
		public void RunInstanceSegmentation(IEnumerable<string> photos, List<Mat> alignedImages, List<Mat> laplacedImages)
		{
			_commandsService.RunModel("2");
			List<List<int>> intensities = new List<List<int>>();
			GetIntensities(photos, alignedImages, laplacedImages, intensities);
			ChooseBestMasks(photos, laplacedImages, intensities);
		}

		private static void ChooseBestMasks(IEnumerable<string> photos, List<Mat> laplacedImages, List<List<int>> intensities)
		{
			for (int i = 0; i < intensities.First().Count(); i++)
			{
				int maxMaskIntensity = 0;
				int index = 0;
				for (int j = 0; j < photos.Count(); j++)
				{
					if (intensities[j][i] > maxMaskIntensity) { maxMaskIntensity = intensities[j][i]; index = j; }
				}
				List<List<Point>> contours = new List<List<Point>>();
				List<Point> currentMask = new List<Point>();

				JArray masksJson = JArray.Parse(File.ReadAllText($"masks{photos.ToArray()[index].Split('\\').Last()}.json"));
				foreach (var points in masksJson[i])
				{
					currentMask.Add(new Point((int)points[0][0], (int)points[0][1]));
				}
				contours.Add(currentMask);
				for (int j = 0; j < laplacedImages.Count(); j++)
				{
					if (j == index)
					{
						Cv2.DrawContours(laplacedImages[j], contours, -1, new Scalar(255, 255, 255), -1);
					}
					else
					{
						Cv2.DrawContours(laplacedImages[j], contours, -1, new Scalar(0, 0, 0), -1);
					}
					laplacedImages[j].SaveImage($"laplace{j}.jpg");
				}

			}
		}

		private static void GetIntensities(IEnumerable<string> photos, List<Mat> alignedImages, List<Mat> laplacedImages, List<List<int>> intensities)
		{
			for (int i = 0; i < photos.Count(); i++)
			{
				Mat imageToMask = alignedImages.ToArray()[i].Clone();

				List<List<Point>> contours = new List<List<Point>>();
				List<Rect> boxes = new List<Rect>();
				JArray masksJson = JArray.Parse(File.ReadAllText($"masks{photos.ToArray()[i].Split('\\').Last()}.json"));
				foreach (var mask in masksJson)
				{
					List<Point> currentMask = new List<Point>();

					foreach (var points in mask)
					{
						currentMask.Add(new Point((int)points[0][0], (int)points[0][1]));
					}
					boxes.Add(Cv2.BoundingRect(currentMask));
					contours.Add(currentMask);
				}

				List<int> currentIntensities = new List<int>();

				for (int j = 0; j < contours.Count; j++)
				{
					int maskIntensity = 0;
					Mat Mask = new Mat(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					Cv2.DrawContours(Mask, contours.GetRange(j, 1), -1, new Scalar(255, 255, 255), -1);
					for (int k = boxes[j].Top; k <= boxes[j].Bottom; k++)
					{
						for (int l = boxes[j].Left; l <= boxes[j].Right; l++)
						{
							if (Mask.At<byte>(k, l) == 255)
							{
								maskIntensity += laplacedImages[i].At<byte>(k, l);
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
