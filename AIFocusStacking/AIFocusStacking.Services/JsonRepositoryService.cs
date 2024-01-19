using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	//Repozytorium plików json
	public class JsonRepositoryService : IRepositoryService<JArray>
	{
		//Folder zawierający pliki
		protected string _repositoryFolder = "jsonFiles";
		public JsonRepositoryService()
		{
			//Stwórz folder
			_ = Directory.CreateDirectory(_repositoryFolder);
		}
		//Dodaj plik do folderu
		//Funkcja kopiuje plik, który już istnieje na dysku
		public ServiceResult Add(string path)
		{
			ServiceResult result = new();
			try
			{
				//Kopiuj plik do folderu
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

		//Dodaj pliki do folderu
		//Funkcja kopiuje pliki, które już istnieją na dysku
		public ServiceResult AddMultiple(string[] paths)
		{
			ServiceResult result = new();
			try
			{
				//Kopiuj pliki do folderu
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

		//Usuń plik z folderu
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

		//Usuń pliki z folderu
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

		//Pobierz tablice ze wszystkich plików
		public IEnumerable<JArray> GetAll()
		{
			IEnumerable<string> paths = Directory.GetFiles(_repositoryFolder);
			foreach (string path in paths) 
			{
				yield return JArray.Parse(File.ReadAllText($"{path}"));
			}
		}

		//Pobierz tablice z wybranych plików
		public IEnumerable<JArray> GetMultiple(string[] names)
		{
			foreach (string name in names)
			{
				yield return JArray.Parse(File.ReadAllText($"{_repositoryFolder}\\{name}"));
			}
		}

		//Pobierz tablice z pliku
		public JArray GetSingle(string name)
		{
			return JArray.Parse(File.ReadAllText($"{_repositoryFolder}\\{name}"));
		}
	}
}
