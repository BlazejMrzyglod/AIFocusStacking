using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIFocusStacking.Services
{
	public class FocusStackingService : IFocusStackingService
	{
		private Mat AlignImages(Mat referenceImage, Mat currentImage)
		{
			Mat grayReference = new Mat();
			Mat grayCurrent = new Mat();
			Cv2.CvtColor(referenceImage, grayReference, ColorConversionCodes.BGR2GRAY);
			Cv2.CvtColor(currentImage, grayCurrent, ColorConversionCodes.BGR2GRAY);

			Mat warpMatrix = Mat.Eye(2, 3, MatType.CV_32F);

			//TODO:Zmienić count na większy
			TermCriteria criteria = new TermCriteria(CriteriaTypes.Count | CriteriaTypes.Eps, 5, 1e-10);

			Cv2.FindTransformECC(grayReference, grayCurrent, warpMatrix, MotionTypes.Euclidean, criteria);

			Mat alignedImage = new Mat();
			Cv2.WarpAffine(currentImage, alignedImage, warpMatrix, referenceImage.Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);

			return alignedImage;
		}

		public ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize, bool takeAll, int maskSize)
		{
			ServiceResult serviceResult = new ServiceResult();
			try
			{
				List<Mat> alignedImages = new List<Mat>();
				List<Mat> laplacedImages = new List<Mat>();
				List<Mat> masks = new List<Mat>();
				// Load the reference image
				Mat referenceImage = new Mat(photos.First());
				alignedImages.Add(referenceImage);
				// Iterate through the rest of the images and align them to the reference image
				for (int i = 0; i < photos.Count(); i++)
				{
					if (i != 0)
					{
						// Load the current image
						Mat currentImage = new Mat(photos.ToArray()[i]);

						// Align the current image to the reference image
						if (alignment)
						{
							Mat alignedImage = AlignImages(referenceImage, currentImage);
							alignedImages.Add(alignedImage);
						}
						else
							alignedImages.Add(currentImage);
					}
					Mat imageToMask = alignedImages.ToArray()[i].Clone();

					List<List<Point>> contours = new List<List<Point>>();
					JArray masksJson = JArray.Parse(File.ReadAllText($"masks{photos.ToArray()[i].Split('\\').Last()}.json"));
					foreach (var mask in masksJson)
					{
						List<Point> currentMask = new List<Point>();

						foreach (var points in mask)
						{
							currentMask.Add(new Point((int)points[0][0], (int)points[0][1]));
						}
						contours.Add(currentMask);
					}
					Mat Mask = new Mat(imageToMask.Size(), imageToMask.Type(), new Scalar(0, 0, 0));
					for (int j = 0; j < contours.Count; j++)
					{
						Cv2.DrawContours(Mask, contours.GetRange(j, 1), -1, new Scalar(255, 255, 255), -1);
					}

					Cv2.BitwiseAnd(imageToMask, Mask, imageToMask);
					masks.Add(Mask);
					Mask.SaveImage($"mask{photos.ToArray()[i].Split('\\').Last()}.jpg");
				}
				Mat matGauss = new Mat();

				for (int i = 0; i < photos.Count(); i++)
				{
					if (gauss)
						Cv2.GaussianBlur(alignedImages.ToArray()[i], matGauss, new OpenCvSharp.Size() { Height = gaussSize, Width = gaussSize }, 0);
					else
						alignedImages.ToArray()[i].CopyTo(matGauss);
					Cv2.CvtColor(matGauss, matGauss, ColorConversionCodes.BGR2GRAY);
					Mat matLaplace = new Mat();
					Cv2.Laplacian(matGauss, matLaplace, -1, laplaceSize);
					laplacedImages.Add(matLaplace);
					matLaplace.SaveImage($"laplace{i}.jpg");
				}
				Mat result = alignedImages.First().Clone();
				byte maxIntensity = 0;
				byte intensity = 0;

				if (!takeAll)
				{
					TakeSinglePixel(alignedImages, laplacedImages, result, ref maxIntensity, ref intensity, maskSize);
				}
				else
				{
					TakeAllPixels(alignedImages, laplacedImages, result, ref maxIntensity, ref intensity, maskSize);
				}


				result.SaveImage("result.jpg");

				

				serviceResult.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				serviceResult.Result = ServiceResultStatus.Error;
				serviceResult.Messages.Add(e.Message);
			}

			return serviceResult;

		}

		private void TakeAllPixels(List<Mat> alignedImages, List<Mat> laplacedImages, Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			for (int i = maskSize; i < result.Rows - maskSize; i += maskSize)
			{
				for (int j = maskSize; j < result.Cols - maskSize; j += maskSize)
				{
					maxIntensity = 0;
					for (int z = 0; z < laplacedImages.Count; z++)
					{
						intensity = 0;
						for (int x = -maskSize; x <= maskSize; x++)
						{
							for (int y = -maskSize; y <= maskSize; y++)
							{
								intensity += laplacedImages.ToArray()[z].At<byte>(i + x, j + y);
							}
						}

						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							for (int x = -maskSize; x <= maskSize; x++)
							{
								for (int y = -maskSize; y <= maskSize; y++)
								{
									result.At<Vec3b>(i + x, j + y) = alignedImages.ToArray()[z].At<Vec3b>(i + x, j + y);
								}
							}

						}
					}

				}
			}
		}

		private void TakeSinglePixel(List<Mat> alignedImages, List<Mat> laplacedImages, Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			for (int i = maskSize; i < result.Rows - maskSize; i++)
			{
				for (int j = maskSize; j < result.Cols - maskSize; j++)
				{
					maxIntensity = 0;
					for (int z = 0; z < laplacedImages.Count; z++)
					{
						intensity = 0;
						for (int x = -maskSize; x <= maskSize; x++)
						{
							for (int y = -maskSize; y <= maskSize; y++)
							{
								intensity += laplacedImages.ToArray()[z].At<byte>(i + x, j + y);
							}
						}

						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							result.At<Vec3b>(i, j) = alignedImages.ToArray()[z].At<Vec3b>(i, j);
						}
					}

				}
			}
		}
	}
}
