namespace AIFocusStacking.Services
{
	//Interfejs repozytorium
	public interface IRepositoryService<T>
	{	
		IEnumerable<T> GetAll(); //Pobierz wszystkie obiekty		
		IEnumerable<T> GetMultiple(string[] names); //Pobierz wiele obiektów		
		T GetSingle(string name); //Pobierz pojedynczy obiekt		
		ServiceResult Add(string path); //Dodaj obiekt		
		ServiceResult AddMultiple(string[] paths); //Dodaj wiele obiektów		
		ServiceResult Delete(string name); //Usuń obiekt		
		ServiceResult DeleteMultiple(string[] names); //Usuń wiele obiektów	
		ServiceResult DeleteAll(); //Usuń wszystkie obiekty
	}
}
