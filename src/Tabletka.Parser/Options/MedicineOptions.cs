using Tabletka.Parser.Models;

namespace Tabletka.Parser.Options;

public class MedicineOptions
{
    public const string Section = "Medicine";
    
    public RequiredMedicineList[] Lists { get; set; }
}