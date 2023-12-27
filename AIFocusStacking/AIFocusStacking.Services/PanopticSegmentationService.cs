using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.FileStorage;

namespace AIFocusStacking.Services
{
	public class PanopticSegmentationService : IPanopticSegmentationService
	{
		protected readonly IConsoleCommandsService _commandsService;
		public PanopticSegmentationService(IConsoleCommandsService commandsService) { _commandsService = commandsService; }
		public void RunPanopticSegmentation(IEnumerable<string> photos, List<Mat> alignedImages, List<Mat> laplacedImages)
		{
			_commandsService.RunModel("3");
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
				List<Point> segment = new List<Point>();
				JArray segmentsJson = JArray.Parse(File.ReadAllText($"panoptic{photos.ToArray()[index].Split('\\').Last()}.json"));
				for (int k = 0; k < segmentsJson.Count; k++)
				{
					for (int l = 0; l < segmentsJson[0].Count(); l++)
					{
						if ((int)segmentsJson[k][l] == i + 1)
						{
							segment.Add(new Point(k, l));
						}
					}
				}

				for (int j = 0; j < laplacedImages.Count(); j++)
				{
					if (j == index)
					{
						for (int k = 0; k < segment.Count; k++)
						{
							laplacedImages[j].At<byte>(segment[k].X, segment[k].Y) = 255;
						}
					}
					else
					{
						for (int k = 0; k < segment.Count; k++)
						{
							laplacedImages[j].At<byte>(segment[k].X, segment[k].Y) = 0;
						}
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

				List<List<Point>> segments = new List<List<Point>>();
				List<Rect> boxes = new List<Rect>();
				JArray segmentsJson = JArray.Parse(File.ReadAllText($"panoptic{photos.ToArray()[i].Split('\\').Last()}.json"));
				JArray segmentsInfo = JArray.Parse(File.ReadAllText($"panoptic_seg_info{photos.ToArray()[i].Split('\\').Last()}.json"));
	
				for (int j = 1; j <= segmentsInfo.Count; j++)
				{
					List<Point> currentSegment = new List<Point>();
					for (int k = 0; k < segmentsJson.Count; k++)
					{
						for(int l = 0; l < segmentsJson[0].Count(); l++)
						{
							if((int)segmentsJson[k][l] == j)
							{
								currentSegment.Add(new Point(k, l));
							}
						}
					}
					boxes.Add(Cv2.BoundingRect(currentSegment));
					segments.Add(currentSegment);
				}

				List<int> currentIntensities = new List<int>();

				for (int j = 0; j < segments.Count; j++)
				{
					int maskIntensity = 0;
					Mat Mask = new Mat(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					for (int k = 0; k < segments[j].Count; k++)
					{
						Mask.At<Vec3b>(segments[j][k].X, segments[j][k].Y) = new Vec3b(255,255,255);
					}
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
