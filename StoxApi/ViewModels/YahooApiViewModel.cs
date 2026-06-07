using CsvHelper.Configuration.Attributes;

public record StockCsvRecord
{
    [Name("Symbol")]
    public string Symbol { get; init; } = default!;
    [Name("Name")]
    public string Name { get; init; } = default!;
    [Name("Last Sale")]
    public string? LastSale { get; init; }
    [Name("Country")]
    public string? Country { get; init; }
    [Name("Sector")]
    public string? Sector { get; init; }
    [Name("Industry")]
    public string? Industry { get; init; }
}