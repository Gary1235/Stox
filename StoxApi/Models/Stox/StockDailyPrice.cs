using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class StockDailyPrice
{
    public Guid Id { get; set; }

    public string StockCode { get; set; } = null!;

    public decimal? OpenPrice { get; set; }

    public decimal? HighPrice { get; set; }

    public decimal? LowPrice { get; set; }

    public decimal? ClosePrice { get; set; }

    public long? Volumn { get; set; }

    public decimal? AdjClose { get; set; }

    public DateTime? CreatedDate { get; set; }
}
