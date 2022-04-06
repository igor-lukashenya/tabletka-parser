namespace Tabletka.Parser.Models;

public class Medicine
{
    public string Name { get; set; }
    public string Form { get; set; }
    public string Produce { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public DateTime Date { get; set; }
}