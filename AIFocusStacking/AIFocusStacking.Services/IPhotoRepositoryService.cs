using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
    public interface IPhotoRepositoryService
    {
        IEnumerable<string> GetAll();
        string GetSingle(string photo);
        ServiceResult Create(string photo);
        ServiceResult CreateMultiple(string[] photo);
        ServiceResult Delete(string photo);
        ServiceResult DeleteMultiple(string[] photo);
        ServiceResult Edit(string photo);
        ServiceResult EditMultiple(string[] photo);
    }
}
