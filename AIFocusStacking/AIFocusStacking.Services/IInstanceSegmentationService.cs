using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	public interface IInstanceSegmentationService
	{
		void RunInstanceSegmentation(IEnumerable<string> photos, List<Mat> alignedImages, List<Mat> laplacedImages);
	}
}
