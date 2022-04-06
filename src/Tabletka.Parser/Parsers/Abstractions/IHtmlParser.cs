using Tabletka.Parser.Models;

namespace Tabletka.Parser.Parsers.Abstractions;

public interface IHtmlParser
{
    IEnumerable<Medicine> Parse(string html);
}