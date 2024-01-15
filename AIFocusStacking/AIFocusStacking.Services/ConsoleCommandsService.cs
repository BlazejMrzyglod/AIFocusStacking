using System.Diagnostics;

namespace AIFocusStacking.Services
{
	//Serwis uruchamiający komendy w konsoli
	public class ConsoleCommandsService : IConsoleCommandsService
	{
		//Repozytorium zdjęć
		protected IRepositoryService<string> _photoRepository;

		//Folder do przechowywania zdjęć z wykrytymi obiektami
		protected string outputDirectory;

		public ConsoleCommandsService(IRepositoryService<string> photoRepository)
		{
			_photoRepository = photoRepository;
			outputDirectory = "outputImages";
		}

		//Funkcja uruchamiająca Detectron2
		public ServiceResult RunModel(string method, string confidence)
		{
			ServiceResult result = new();
			try
			{
				//Pobierz zdjęcia
				IEnumerable<string> photos = _photoRepository.GetAll();

				//Określ argumenty uruchamianego procesu
				ProcessStartInfo start = new();

				//Uruchamiany skrypt
				string script = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\demo.py";

				//Plik konfiguracyjny modelu
				string configFile;

				//Plik z wagami modelu
				string weights;

				//Ustawienia dla segmentacji instancji
				if (method == "2")
				{
					configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\configs\\Misc\\cascade_mask_rcnn_X_152_32x8d_FPN_IN5k_gn_dconv.yaml";//\\COCO-InstanceSegmentation\\mask_rcnn_X_101_32x8d_FPN_3x.yaml";
					weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_0039999_e76410.pkl";// model_final_2d9806.pkl";
				}

				//Ustawienia dla panoptycznej segmentacji
				else
				{
					configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\configs\\Misc\\panoptic_fpn_R_101_dconv_cascade_gn_3x.yaml";//\\COCO-PanopticSegmentation\\panoptic_fpn_R_101_3x.yaml";
					weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_final_be35db.pkl";// model_final_cafdb1.pkl";
				}

				//Stwórz folder wyjściowy
				_ = Directory.CreateDirectory(outputDirectory);

				//Dodatkowe opcje dla uruchamianego skryptu
				string options = "MODEL.DEVICE cpu";

				//Uruchom Detectron2 
				start.FileName = "CMD.exe";
				start.Arguments = $"/C python {script} --config-file {configFile} --input {string.Join(" ", photos)} --output {outputDirectory} --confidence-threshold {confidence} --opts {options} MODEL.WEIGHTS {weights}";
				start.UseShellExecute = false;
				start.RedirectStandardOutput = true;
				start.CreateNoWindow = true;

				using (Process process = Process.Start(start)!)
				{
					using StreamReader reader = process!.StandardOutput;
					string consoleResult = reader.ReadToEnd();
				}
				//_photoRepository.DeleteMultiple(photos.ToArray());

				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}

			return result;
		}

		//Funkcja czyszczące folder z wyjściowymi zdjęciami
		public ServiceResult ClearOutputDirectory()
		{
			ServiceResult result = new();
			try
			{
				Directory.Delete(outputDirectory, true);
				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}

			return result;
		}
	}
}
