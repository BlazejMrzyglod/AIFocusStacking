namespace AIFocusStacking.Services
{
	//Repozytorium zdjęć
	public class PhotoRepositoryService : IRepositoryService<string>
	{
		//Folder zawierający zdjęcia
		protected string _repositoryFolder = "images";

		public PhotoRepositoryService()
		{
			//Stwórz folder
			_ = Directory.CreateDirectory(_repositoryFolder);
		}
		//Dodaj zdjęcie do folderu
		//Funkcja kopiuje zdjęcie, które już istnieje na dysku
		public ServiceResult Add(string path)
		{
			ServiceResult result = new();
			try
			{
				//Kopiuj zdjęcie do folderu
				File.Copy(path, _repositoryFolder + "\\" + path.Split("\\").Last());

				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}
			return result;
		}

		//Dodaj zdjęcia do folderu
		//Funkcja kopiuje zdjęcia, które już istnieją na dysku
		public ServiceResult AddMultiple(string[] paths)
		{
			ServiceResult result = new();
			try
			{
				//Kopiuj zdjęcia do folderu
				foreach (string path in paths)
				{
					File.Copy(path, _repositoryFolder + "\\" + path.Split("\\").Last());
				}

				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}

			return result;

		}

		//Usuń zdjęcie z folderu
		public ServiceResult Delete(string name)
		{
			ServiceResult result = new();
			try
			{
				File.Delete($"{_repositoryFolder}\\{name}");
				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}
			return result;
		}

		//Usuń zdjęcia z folderu
		public ServiceResult DeleteMultiple(string[] names)
		{
			ServiceResult result = new();
			try
			{
				foreach (string name in names)
				{
					File.Delete($"{_repositoryFolder}\\{name}");
				}
				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}
			return result;
		}

		//Usuń cały folder
		public ServiceResult DeleteAll()
		{
			ServiceResult result = new();
			try
			{
				foreach (string file in Directory.GetFiles(_repositoryFolder))
				{
					File.Delete(file);
				}
				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}
			return result;
		}

		//Pobierz adres wszystkich zdjęć
		public IEnumerable<string> GetAll()
		{
			IEnumerable<string> paths = Directory.GetFiles(_repositoryFolder);
			return paths;
		}

		//Pobierz adres wielu zdjęć
		public IEnumerable<string> GetMultiple(string[] names)
		{
			return Directory.GetFiles(_repositoryFolder).Where(r => names.Contains(r.Split("\\").Last()));
		}

		//Pobierz adres zdjęci
		public string GetSingle(string name)
		{
			return Directory.GetFiles(_repositoryFolder).Where(r => r.Split("\\").Last() == name).SingleOrDefault()!;
		}
	}
}
