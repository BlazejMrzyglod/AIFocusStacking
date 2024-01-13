using AIFocusStacking.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
	public class JsonRepositoryService : IRepositoryService<JArray>
	{
		protected string _repositoryFolder = "jsonFiles";

		public ServiceResult Add(string path)
		{
			ServiceResult result = new();
			try
			{
				//Stwórz folder
				_ = Directory.CreateDirectory(_repositoryFolder);

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

		public ServiceResult AddMultiple(string[] paths)
		{
			ServiceResult result = new();
			try
			{
				// Stwórz folder
				_ = Directory.CreateDirectory(_repositoryFolder);

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

		public ServiceResult DeleteAll()
		{
			ServiceResult result = new();
			try
			{
				Directory.Delete(_repositoryFolder, true);
				result.Result = ServiceResultStatus.Succes;
			}
			catch (Exception e)
			{
				result.Result = ServiceResultStatus.Error;
				result.Messages.Add(e.Message);
			}
			return result;
		}

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

		public IEnumerable<JArray> GetAll()
		{
			IEnumerable<string> paths = Directory.GetFiles(_repositoryFolder);
			foreach (string path in paths) 
			{
				yield return JArray.Parse(File.ReadAllText($"{path}"));
			}
		}

		public IEnumerable<JArray> GetMultiple(string[] names)
		{
			foreach (string name in names)
			{
				yield return JArray.Parse(File.ReadAllText($"{_repositoryFolder}\\{name}"));
			}
		}

		public JArray GetSingle(string name)
		{
			return JArray.Parse(File.ReadAllText($"{_repositoryFolder}\\{name}"));
		}
	}
}
