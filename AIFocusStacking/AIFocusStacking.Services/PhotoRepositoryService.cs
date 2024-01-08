namespace AIFocusStacking.Services
{
	//Repozytorium zdjęć
	public class PhotoRepositoryService : IPhotoRepositoryService
    {
        //Folder zawierający zdjęcia
        protected string _repositoryFolder = "images";

        //Stwórz zdjęcie w folderze
        public ServiceResult Create(string photo)
        {
            ServiceResult result = new();
            try
            {
                //Stwórz folder
                Directory.CreateDirectory(_repositoryFolder);

                //Kopiuj zdjęcie do folderu
                File.Copy(photo, _repositoryFolder + "\\" + photo.Split("\\").Last());

                result.Result = ServiceResultStatus.Succes;
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }
            return result;
        }

		//Stwórz zdjęcia w folderze
		public ServiceResult CreateMultiple(string[] photos)
        {
            ServiceResult result = new();
            try
            {
				//Stwórz folder
				Directory.CreateDirectory(_repositoryFolder);

				//Kopiuj zdjęcia do folderu
				foreach (var photo in photos)
                {
                    File.Copy(photo, _repositoryFolder + "\\" + photo.Split("\\").Last());
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
        public ServiceResult Delete(string photo)
        {
            ServiceResult result = new();
            try
            {
                File.Delete(_repositoryFolder + "\\" + photo.Split("\\").Last());
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
		public ServiceResult DeleteMultiple(string[] photos)
        {
            ServiceResult result = new();
            try
            {
                foreach (var photo in photos)
                {
                    File.Delete(_repositoryFolder + "\\" + photo.Split("\\").Last());
                    result.Result = ServiceResultStatus.Succes;
                }          
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }
            return result;
        }

        //Usuń wszystkie zdjęcia
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

        //Edytuj zdjęcie
        public ServiceResult Edit(string photo)
        {
            ServiceResult result = new();
            try
            {        
                //Usuń zdjęcie
                Delete(photo);

                //Stwórz zdjęcie
                Create(photo);

                result.Result = ServiceResultStatus.Succes;
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }
            return result;
        }

		//Edytuj zdjęcia
		public ServiceResult EditMultiple(string[] photos)
        {
            ServiceResult result = new();
            try
            {
                //Iteruj po zdjęciach
                foreach (var photo in photos)
                {
					//Usuń zdjęcie
					Delete(photo);

					//Stwórz zdjęcie
					Create(photo);

                    result.Result = ServiceResultStatus.Succes;
                }            
            }
            catch (Exception e)
            {
                result.Result = ServiceResultStatus.Error;
                result.Messages.Add(e.Message);
            }
            return result;
        }

        //Pobierz wszystkie zdjęcia
        public IEnumerable<string> GetAll()
        {
            IEnumerable<string> photos = Directory.GetFiles(_repositoryFolder);
            return photos;
        }

        //Pobierz zdjęcie
        public string GetSingle(string photo)
        {
            return Directory.GetFiles(_repositoryFolder).Where(r => r == photo).SingleOrDefault()!;
        }

        //Zmień folder zawierający zdjęcia
        public ServiceResult ChangeDirectory(string directory)
        {
            ServiceResult result = new();
            try
            {
                _repositoryFolder = directory;

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
