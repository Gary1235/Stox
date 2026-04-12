using System.Text.Json.Serialization;

public record StockQuote(
    [property: JsonPropertyName("c")] decimal CurrentPrice,
    [property: JsonPropertyName("h")] decimal HighPrice,
    [property: JsonPropertyName("l")] decimal LowPrice,
    [property: JsonPropertyName("o")] decimal OpenPrice,
    [property: JsonPropertyName("pc")] decimal PreviousClose
);
