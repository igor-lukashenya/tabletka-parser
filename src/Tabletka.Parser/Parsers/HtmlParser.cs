using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Tabletka.Parser.Models;
using Tabletka.Parser.Options;
using Tabletka.Parser.Parsers.Abstractions;

namespace Tabletka.Parser.Parsers;

public class HtmlParser : IHtmlParser
{
    private readonly IOptions<ParsingOptions> _options;

    public HtmlParser(IOptions<ParsingOptions> options)
    {
        _options = options;
    }

    public IEnumerable<Medicine> Parse(string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var table = htmlDocument.GetElementbyId("base-select");
        return table.Descendants("tr")
            .Where(r => r.HasClass("tr-border"))
            .Select(ParseRow)
            .ToList();
    }

    private Medicine ParseRow(HtmlNode row)
    {
        var culture = !string.IsNullOrWhiteSpace(_options.Value.Culture)
            ? CultureInfo.GetCultureInfo(_options.Value.Culture)
            : CultureInfo.CurrentCulture;

        var price = row.Descendants("td")
            .FirstOrDefault(td => td.HasClass("price"))?
            .Descendants("span")
            .FirstOrDefault(span => span.HasClass("price-value"))?
            .InnerText.Trim();

        var prices = !string.IsNullOrEmpty(price)
            ? Regex.Matches(price, @"\d+.\d+").Select(match => match.Value).ToArray()
            : new[] { "no price", "no price" };
        return new Medicine
        {
            Name = row.Descendants("td")
                .FirstOrDefault(td => td.HasClass("name"))?
                .Descendants("div")
                .FirstOrDefault(div => div.HasClass("tooltip-info-header"))?
                .InnerText.Trim(),

            Form = row.Descendants("td")
                .FirstOrDefault(td => td.HasClass("form"))?
                .Descendants("div")
                .FirstOrDefault(div => div.HasClass("tooltip-info-header"))?
                .InnerText.Trim(),

            Produce = row.Descendants("td")
                .FirstOrDefault(td => td.HasClass("produce"))?
                .Descendants("div")
                .FirstOrDefault(div => div.HasClass("tooltip-info-header"))?
                .InnerText.Trim(),

            MinPrice = decimal.TryParse(prices.First(), NumberStyles.Number, culture, out var minPrice) ? minPrice : 0,
            MaxPrice = decimal.TryParse(prices.Last(), NumberStyles.Number, culture, out var maxPrice) ? maxPrice : 0
        };
    }
}