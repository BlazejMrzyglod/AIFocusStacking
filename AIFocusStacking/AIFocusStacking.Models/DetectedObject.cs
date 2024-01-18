using OpenCvSharp;

namespace AIFocusStacking.Models
{
	//Model z informacjami na temat wykrytego obiektu
	public class DetectedObject
	{
		//Lista punktów należących do obiektu
		//Może być to kontur lub lista wszystkich punktów
		public List<Point> Mask { get; set; }

		//Ramka ograniczająca obiektu
		public Rect Box { get; set; }

		//Klasa obiektu wykryta przez SI
		public int Class { get; set; }

		//Średnia intensywność pikseli w obszarze obiektu
		public int? Intensity { get; set; }

		//Konstruktor
		public DetectedObject(List<Point> mask, Rect box, int _class)
		{
			Mask = mask;
			Box = box;
			Class = _class;
		}
	}
}
