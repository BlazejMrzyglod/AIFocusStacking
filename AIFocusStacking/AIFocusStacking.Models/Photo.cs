using OpenCvSharp;

namespace AIFocusStacking.Models
{
	public class Photo
	{
        public Mat Matrix { get; set; }
		public string Path { get; set; }
		public Mat? MatrixAfterLaplace { get; set; }
        public List<DetectedObject>? DetectedObjects { get; set; }    

        public Photo(Mat matrix, string path)
        {
            Matrix = matrix;
            Path = path;
        }
		
    }
}
