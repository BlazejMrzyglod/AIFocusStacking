namespace AIFocusStacking.Services
{
	//Interfejs repozytorium
	public interface IRepositoryService<T>
	{
		IEnumerable<T> GetAll();
		IEnumerable<T> GetMultiple(string[] names);
		T GetSingle(string name);
		ServiceResult Add(string path);
		ServiceResult AddMultiple(string[] paths);
		ServiceResult Delete(string name);
		ServiceResult DeleteMultiple(string[] names);
		ServiceResult DeleteAll();
	}
}
