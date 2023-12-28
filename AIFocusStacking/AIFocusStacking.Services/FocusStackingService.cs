using AIFocusStacking.Models;
using OpenCvSharp;

namespace AIFocusStacking.Services
{
	public class FocusStackingService : IFocusStackingService
	{
		protected readonly IInstanceSegmentationService _instanceSegmentationService;
		protected readonly IPanopticSegmentationService _panopticSegmentationService;
		protected List<Photo> _photos;
		public FocusStackingService(IInstanceSegmentationService instanceSegmentationService, IPanopticSegmentationService panopticSegmentationService) 
		{ 
			_instanceSegmentationService = instanceSegmentationService; 
			_panopticSegmentationService = panopticSegmentationService;
			_photos = new List<Photo>();
		}
		private static Mat AlignImages(Mat referenceImage, Mat currentImage)
		{
			Mat grayReference = new();
			Mat grayCurrent = new();
			Cv2.CvtColor(referenceImage, grayReference, ColorConversionCodes.BGR2GRAY);
			Cv2.CvtColor(currentImage, grayCurrent, ColorConversionCodes.BGR2GRAY);

			Mat warpMatrix = Mat.Eye(2, 3, MatType.CV_32F);

			TermCriteria criteria = new(CriteriaTypes.Count | CriteriaTypes.Eps, 500, 1e-10);

			Cv2.FindTransformECC(grayReference, grayCurrent, warpMatrix, MotionTypes.Euclidean, criteria);

			Mat alignedImage = new();
			Cv2.WarpAffine(currentImage, alignedImage, warpMatrix, referenceImage.Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);

			return alignedImage;
		}

		public ServiceResult RunFocusStacking(IEnumerable<string> photos, bool alignment, bool gauss, int laplaceSize, int gaussSize, 
											  bool takeAll, int maskSize, string method)
		{
			ServiceResult serviceResult = new();
			try
			{
				// Load the reference image
				Mat referenceImage = new(photos.First());
				_photos.Add(new Photo() { Path = photos.First(), Matrix = referenceImage });
				// Iterate through the rest of the images and align them to the reference image
				for (int i = 1; i < photos.Count(); i++)
				{
					// Load the current image
					Mat currentImage = new(photos.ToArray()[i]);

					// Align the current image to the reference image
					if (alignment)
					{
						Mat alignedImage = AlignImages(referenceImage, currentImage);
						_photos.Add(new Photo() { Path = photos.ToArray()[i], Matrix = alignedImage });
					}
					else
					{
						_photos.Add(new Photo() { Path = photos.ToArray()[i], Matrix = currentImage });
					}
				}
				Mat matGauss = new();

				for (int i = 0; i < _photos.Count; i++)
				{
					if (gauss)
						Cv2.GaussianBlur(_photos[i].Matrix!, matGauss, new Size() { Height = gaussSize, Width = gaussSize }, 0);
					else
						_photos[i].Matrix!.CopyTo(matGauss);

					_photos[i].MatrixAfterGauss = matGauss;

					Cv2.CvtColor(matGauss, matGauss, ColorConversionCodes.BGR2GRAY);
					Mat matLaplace = new();
					Cv2.Laplacian(matGauss, matLaplace, -1, laplaceSize);
					_photos[i].MatrixAfterLaplace = matLaplace;
					matLaplace.SaveImage($"laplace{i}.jpg");

				}

				if (method == "2")
				{
					_instanceSegmentationService.RunInstanceSegmentation(_photos);
				}
				else if (method == "3")
				{
					_panopticSegmentationService.RunPanopticSegmentation(_photos);
				}

				Mat result =_photos.First().Matrix!.Clone();
				byte maxIntensity = 0;
				byte intensity = 0;

				if (!takeAll)
				{
					TakeSinglePixel(result, ref maxIntensity, ref intensity, maskSize);
				}
				else
				{
					TakeAllPixels(result, ref maxIntensity, ref intensity, maskSize);
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

		private void TakeAllPixels(Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			for (int i = maskSize; i < result.Rows - maskSize; i += maskSize)
			{
				for (int j = maskSize; j < result.Cols - maskSize; j += maskSize)
				{
					maxIntensity = 0;
					for (int z = 0; z < _photos.Count; z++)
					{
						intensity = 0;
						for (int x = -maskSize; x <= maskSize; x++)
						{
							for (int y = -maskSize; y <= maskSize; y++)
							{
								intensity += _photos[z].MatrixAfterLaplace!.At<byte>(i + x, j + y);
							}
						}

						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							for (int x = -maskSize; x <= maskSize; x++)
							{
								for (int y = -maskSize; y <= maskSize; y++)
								{
									result.At<Vec3b>(i + x, j + y) = _photos[z].Matrix!.At<Vec3b>(i + x, j + y);
								}
							}

						}
					}

				}
			}
		}

		private void TakeSinglePixel(Mat result, ref byte maxIntensity, ref byte intensity, int maskSize)
		{
			for (int i = maskSize; i < result.Rows - maskSize; i++)
			{
				for (int j = maskSize; j < result.Cols - maskSize; j++)
				{
					maxIntensity = 0;
					for (int z = 0; z < _photos.Count; z++)
					{
						intensity = 0;
						for (int x = -maskSize; x <= maskSize; x++)
						{
							for (int y = -maskSize; y <= maskSize; y++)
							{
								intensity += _photos[z].MatrixAfterLaplace!.At<byte>(i + x, j + y);
							}
						}

						if (intensity > maxIntensity)
						{
							maxIntensity = intensity;
							result.At<Vec3b>(i, j) = _photos[z].Matrix!.At<Vec3b>(i, j);
						}
					}

				}
			}
		}
	}
}
