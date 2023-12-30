using OpenCvSharp;

namespace AIFocusStacking.Models
{
	public class DetectedObject
	{
		public List<Point> Mask { get; set; }
		public Rect Box { get; set; }
        public int Class { get; set; }
        public int? Intensity { get; set; }

        public DetectedObject(List<Point> mask, Rect box, int _class) 
        {
            Mask = mask;
            Box = box;
            Class = _class;
        }
    }
}
