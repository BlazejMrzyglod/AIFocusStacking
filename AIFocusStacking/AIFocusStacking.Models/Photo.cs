using OpenCvSharp;

namespace AIFocusStacking.Models
{
	//Model z informacjami na temat zdjęcia
	public class Photo
	{
		//Oryginalne zdjęcie
		public Mat Matrix { get; set; }

		//Nazwa zdjęcia
		public string Name { get; set; }

		//Zdjęcie po filtrze Laplace'a
		public Mat? MatrixAfterLaplace { get; set; }

		//Kolekcja obiektów wykrytych na tym zdjęciu
		public List<DetectedObject>? DetectedObjects { get; set; }

		//Konstruktor
		public Photo(Mat matrix, string name)
		{
			Matrix = matrix;
			Name = name;
		}

	}
}
