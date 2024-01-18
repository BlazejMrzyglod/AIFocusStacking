using System.ComponentModel;

namespace AIFocusStacking.Services
{
	//Status obiektu zwracanego przez serwisy
	public enum ServiceResultStatus
	{
		[Description("Błąd")]
		Error = 0,
		[Description("Sukces")]
		Succes = 1,
		[Description("Ostrzeżenie")]
		Warrnig,
		[Description("Informacja")]
		Info,
	}

	//Obiekt zwracany przez serwisy
	public class ServiceResult
	{
		//Status obiektu
		public ServiceResultStatus Result { get; set; }

		//Wiadomość zwracana
		public ICollection<string> Messages { get; set; }

		//Konstruktor
		public ServiceResult()
		{
			Result = ServiceResultStatus.Succes;
			Messages = new List<string>();
		}
	}
}
