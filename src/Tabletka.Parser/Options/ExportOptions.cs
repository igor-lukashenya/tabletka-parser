namespace Tabletka.Parser.Options;

public class ExportOptions
{
    public const string Section = "Export";
    
    public string TargetDirectory { get; set; }
    public string FileNameDateFormat { get; set; }
    public string Culture { get; set; }
    public string DateFormat { get; set; }
}