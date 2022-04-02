using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Tabletka.Parser.Models;

public class Result
{
    public string Name { get; set; }
    public string Form { get; set; }
    public string Produce { get; set; }
    public string MinPrice { get; set; }
    public string MaxPrice { get; set; }
    public DateOnly Date { get; set; }

    public static Result Create(HtmlNode row, DateOnly date)
    {
        var price = row.Descendants("td")
            .FirstOrDefault(td => td.HasClass("price"))?
            .Descendants("span")
            .FirstOrDefault(span => span.HasClass("price-value"))?
            .InnerText.Trim();

        var prices = !string.IsNullOrEmpty(price)
            ? Regex.Matches(price, @"\d+.\d+").Select(match => match.Value).ToArray()
            : new[] { "no price", "no price" };
        return new Result
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

            MinPrice = prices.First(),
            MaxPrice = prices.Last(),
            Date = date
        };
    }
}