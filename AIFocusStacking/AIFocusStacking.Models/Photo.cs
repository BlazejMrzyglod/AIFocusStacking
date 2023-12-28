using OpenCvSharp;

namespace AIFocusStacking.Models
{
	public class Photo
	{
        public Mat? Matrix { get; set; }
        public Mat? MatrixAfterGauss { get; set; }
        public Mat? MatrixAfterLaplace { get; set; }
        public List<DetectedObject>? DetectedObjects { get; set; }
        public string? Path { get; set; }
		
    }
}
