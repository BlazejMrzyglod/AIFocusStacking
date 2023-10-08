using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
    public class PhotoRepository : IPhotoRepository
    {
        public ServiceResult Create(string photo)
        {
            throw new NotImplementedException();
        }

        public ServiceResult CreateMultiple(string[] photos)
        {
            ServiceResult result = new ServiceResult();
            try
            {
                int i = 0;
                foreach (var photo in photos)
                {
                    Directory.CreateDirectory("images");
                    File.Copy(photo, "images\\photo" + i + ".jpg"); //TODO: Rozszerzenie powinno być zmienne
                    i++;
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
            throw new NotImplementedException();
        }

        public ServiceResult DeleteMultiple(string[] photos)
        {
            throw new NotImplementedException();
        }

        public ServiceResult Edit(string photo)
        {
            throw new NotImplementedException();
        }

        public ServiceResult EditMultiple(string[] photos)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAll()
        {
            throw new NotImplementedException();
        }

        public string GetSingle(string photo)
        {
            throw new NotImplementedException();
        }
    }
}
