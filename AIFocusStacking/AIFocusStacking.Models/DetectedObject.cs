using OpenCvSharp;

namespace AIFocusStacking.Models
{
	public class DetectedObject
	{
        public Rect? Box { get; set; }
        public List<Point>? Mask { get; set; }
        public int? Class { get; set; }
        public int? Intensity { get; set; }
    }
}
