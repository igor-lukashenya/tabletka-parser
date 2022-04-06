namespace Tabletka.Parser.Models;

public class RequiredMedicine
{
    public string Name { get; set; }
    public bool StrictName { get; set; }
    public string[] Produces { get; set; }
    public string[] Forms { get; set; }
}