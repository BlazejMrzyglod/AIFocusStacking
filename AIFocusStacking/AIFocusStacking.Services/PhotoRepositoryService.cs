namespace AIFocusStacking.Services
{
    public class PhotoRepositoryService : IPhotoRepositoryService
    {
        protected string _repositoryFolder = "images";
        public ServiceResult Create(string photo)
        {
            ServiceResult result = new();
            try
            {
                Directory.CreateDirectory(_repositoryFolder);
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

        public ServiceResult CreateMultiple(string[] photos)
        {
            ServiceResult result = new();
            try
            {
                Directory.CreateDirectory(_repositoryFolder);
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
        public ServiceResult Edit(string photo)
        {
            ServiceResult result = new();
            try
            {
                Directory.CreateDirectory(_repositoryFolder);
                File.Delete(_repositoryFolder + "\\" + photo.Split("\\").Last());
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

        public ServiceResult EditMultiple(string[] photos)
        {
            ServiceResult result = new();
            try
            {
                foreach (var photo in photos)
                {
                    Directory.CreateDirectory(_repositoryFolder);
                    File.Delete(_repositoryFolder + "\\" + photo.Split("\\").Last());
                    File.Copy(photo, _repositoryFolder + "\\" + photo.Split("\\").Last());
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

        public IEnumerable<string> GetAll()
        {
            IEnumerable<string> photos = Directory.GetFiles(_repositoryFolder);
            return photos;
        }

        public string GetSingle(string photo)
        {
            return Directory.GetFiles(_repositoryFolder).Where(r => r == photo).SingleOrDefault()!;
        }

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
