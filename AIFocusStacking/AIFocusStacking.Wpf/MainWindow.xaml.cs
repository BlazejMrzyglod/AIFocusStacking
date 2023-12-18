using AIFocusStacking.Services;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AIFocusStacking.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window
	{
		protected readonly IPhotoRepositoryService _photoRepository;
		protected readonly IConsoleCommandsService _commandsService;
		private bool? alignment;
		private bool? gauss;
		private bool? takeAll;
		private int gaussSize;
		private int laplaceSize;
		private int maskSize;
		public MainWindow(IPhotoRepositoryService photoRepository, IConsoleCommandsService commandsService)
		{
			_photoRepository = photoRepository;
			_commandsService = commandsService;
			InitializeComponent();
			alignment = Alignment.IsChecked;
			gauss = Gauss.IsChecked;
			takeAll = TakeAll.IsChecked;
			gaussSize = Convert.ToInt32(GaussSize.Text);
			laplaceSize = Convert.ToInt32(LaplaceSize.Text);
			maskSize = Convert.ToInt32(MaskSize.Text);
		}

		//Funkcja umożliwająca wybranie zdjęć
		private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;
			if (fileDialog.ShowDialog() == true)
				_photoRepository.CreateMultiple(fileDialog.FileNames);
			foreach (var file in fileDialog.FileNames)
				ImagesWrapPanel.Children.Add(new System.Windows.Controls.Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
		}

		//Funkcja odpowiadająca za dodawanie "przeciągniętych" zdjęć
		private void ImagesWrapPanel_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				_photoRepository.CreateMultiple(files);
				foreach (var file in files)
					ImagesWrapPanel.Children.Add(new System.Windows.Controls.Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
			}
		}

		//Funkcja uruchamiająca detectrona w konsoli
		//TODO: Przenieść do serwisów
		private void RunModelButton_Click(object sender, RoutedEventArgs e)
		{
			Mat mat = new Mat(_photoRepository.GetAll().First());
			_commandsService.RunModel();
			List<List<OpenCvSharp.Point>> contours = new List<List<OpenCvSharp.Point>>();
			JArray masksJson = JArray.Parse(File.ReadAllText("masks.json"));
			foreach (var mask in masksJson)
			{
				List<OpenCvSharp.Point> currentMask = new List<OpenCvSharp.Point>();
				
				foreach (var points in mask)
				{
					currentMask.Add(new OpenCvSharp.Point((int)points[0][0], (int)points[0][1]));
				}
				contours.Add(currentMask);
			}
			Mat Mask = new Mat(mat.Size(), mat.Type(), new Scalar(0, 0, 0));
			for (int i = 0; i < contours.Count; i++)
			{
				Cv2.DrawContours(Mask, contours.GetRange(i, 1), -1, new Scalar(255, 255, 255), -1);
			}
			
			Cv2.BitwiseAnd(mat, Mask, mat);



			Mask.SaveImage("tes.jpg");

			ObjectsWindow objectsWindow = new ObjectsWindow();
			objectsWindow.Show();
		}

		private void RunFocusStacking_Click(object sender, RoutedEventArgs e)
		{
			IEnumerable<string> photos = _photoRepository.GetAll();
			List<Mat> alignedImages = new List<Mat>();
			List<Mat> laplacedImages = new List<Mat>();
			// Load the reference image
			Mat referenceImage = new Mat(photos.First());
			alignedImages.Add(referenceImage);
			// Iterate through the rest of the images and align them to the reference image
			for (int i = 1; i < photos.Count(); i++)
			{
				// Load the current image
				Mat currentImage = new Mat(photos.ToArray()[i]);

				// Align the current image to the reference image
				if ((bool)alignment)
				{
					Mat alignedImage = AlignImages(referenceImage, currentImage);
					alignedImages.Add(alignedImage);
				}
				else
					alignedImages.Add(currentImage);
			}
			Mat matGauss = new Mat();

			for (int i = 0; i < photos.Count(); i++)
			{
				if ((bool)gauss)
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

			if (!(bool)takeAll)
			{
				TakeSinglePixel(alignedImages, laplacedImages, result, ref maxIntensity, ref intensity);
			}
			else
			{
				TakeAllPixels(alignedImages, laplacedImages, result, ref maxIntensity, ref intensity);
			}

			
			result.SaveImage("result.jpg");


		}

		private void TakeAllPixels(List<Mat> alignedImages, List<Mat> laplacedImages, Mat result, ref byte maxIntensity, ref byte intensity)
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

		private void TakeSinglePixel(List<Mat> alignedImages, List<Mat> laplacedImages, Mat result, ref byte maxIntensity, ref byte intensity)
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

		static Mat AlignImages(Mat referenceImage, Mat currentImage)
		{
			// Convert images to grayscale
			Mat grayReference = new Mat();
			Mat grayCurrent = new Mat();
			Cv2.CvtColor(referenceImage, grayReference, ColorConversionCodes.BGR2GRAY);
			Cv2.CvtColor(currentImage, grayCurrent, ColorConversionCodes.BGR2GRAY);

			// Set the warp matrix to identity as the initial guess
			Mat warpMatrix = Mat.Eye(2, 3, MatType.CV_32F);

			// Set termination criteria for the ECC algorithm TODO: Change Count to 5000 or smth else
			TermCriteria criteria = new TermCriteria(CriteriaTypes.Count | CriteriaTypes.Eps, 5, 1e-10);

			// Find the optimal transformation
			Cv2.FindTransformECC(grayReference, grayCurrent, warpMatrix, MotionTypes.Euclidean, criteria);

			// Apply the transformation to align the images
			Mat alignedImage = new Mat();
			Cv2.WarpAffine(currentImage, alignedImage, warpMatrix, referenceImage.Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);

			return alignedImage;
		}

		private void Alignment_Click(object sender, RoutedEventArgs e)
		{
			alignment = Alignment.IsChecked;
		}

		private void Gauss_Click(object sender, RoutedEventArgs e)
		{
			gauss = Gauss.IsChecked;
		}

		private void TakeAll_Click(object sender, RoutedEventArgs e)
		{
			takeAll = TakeAll.IsChecked;
		}

		private void GaussSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (GaussSize.Text.Length > 0)
				gaussSize = Convert.ToInt32(GaussSize.Text);
		}

		private void LaplaceSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (LaplaceSize.Text.Length > 0)
				laplaceSize = Convert.ToInt32(LaplaceSize.Text);
		}

		private void MaskSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (MaskSize.Text.Length > 0)
				maskSize = Convert.ToInt32(MaskSize.Text);
		}
	}

}
