namespace AIFocusStacking.Services
{
	//Interfejs repozytorium zdjęć
	public interface IPhotoRepositoryService
	{
		IEnumerable<string> GetAll();
		string GetSingle(string photo);
		ServiceResult Create(string photo);
		ServiceResult CreateMultiple(string[] photo);
		ServiceResult Delete(string photo);
		ServiceResult DeleteMultiple(string[] photo);
		ServiceResult DeleteAll();
		ServiceResult Edit(string photo);
		ServiceResult EditMultiple(string[] photo);
	}
}
