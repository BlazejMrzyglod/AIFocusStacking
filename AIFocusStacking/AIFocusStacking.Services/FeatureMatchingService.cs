using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	public class FeatureMatchingService : IFeatureMatchingService
	{
		public int GetAmountOfMatches(Mat img1, Mat img2)
		{
			BRISK briskDetector = BRISK.Create();
			KeyPoint[] keyPoints1;
			Mat descriptors1 = new();
			briskDetector.DetectAndCompute(img1, null, out keyPoints1, descriptors1);

			KeyPoint[] keyPoints2;
			Mat descriptors2 = new();
			briskDetector.DetectAndCompute(img2, null, out keyPoints2, descriptors2);

			BFMatcher bfMatcher = new BFMatcher(NormTypes.Hamming, true);

			DMatch[] matches = bfMatcher.Match(descriptors1, descriptors2);

			return matches.Length;
		}
	}
}
