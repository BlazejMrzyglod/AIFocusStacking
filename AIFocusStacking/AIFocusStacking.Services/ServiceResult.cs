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
        public ICollection<String> Messages { get; set; }

        public ServiceResult()
        {
            Result = ServiceResultStatus.Succes;
            Messages = new List<string>();
        }

        //Słownik tworzący obiekt zwracany zależnie od wiadomości
        public static Dictionary<string, ServiceResult> CommonResults { get; set; } = new Dictionary<string, ServiceResult>()
        {
              {"NotFound" , new ServiceResult() {
                  Result =ServiceResultStatus.Error,
                  Messages = new List<string>( new string[] { "Nie znaleziono pliku" })  } },
              {"OK" , new ServiceResult() {
                  Result =ServiceResultStatus.Succes,
                  Messages = new List<string>()  } }
        };
    }
}
