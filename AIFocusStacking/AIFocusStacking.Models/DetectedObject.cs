using OpenCvSharp;

namespace AIFocusStacking.Models
{
    //Model z informacjami na temat wykrytego obiektu
	public class DetectedObject
	{
        //Lista punktów należących do obiektu
        //Może być to kontur lub lista wszystkich punktów
		public List<Point> Mask { get; set; }

        //Ramka ograniczająca obiekt
		public Rect Box { get; set; }

        //Klasa obiektu wykryta przez AI
        public int Class { get; set; }

        //Ogólna intensywność pikseli w obszarze obiektu
        public int? Intensity { get; set; }

        public DetectedObject(List<Point> mask, Rect box, int _class) 
        {
            Mask = mask;
            Box = box;
            Class = _class;
        }
    }
}
