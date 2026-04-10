using System.ComponentModel;

public class StockSearchViewModel : ISearchCommon
{
    public StockSearchViewModel()
    {
        Page = 1;
    }

    public string? Keyword { get; set; }

    public int Page { get; set; }
}

public class StockViewModel
{
    /// <summary>
    /// 股票代碼
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// 股票名稱
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 持有股數
    /// </summary>
    public decimal Quantity { get; set; }
    /// <summary>
    /// 平均成本價格
    /// </summary>
    public decimal AvgCost { get; set; }
    /// <summary>
    /// 最新收盤價格
    /// </summary>
    public decimal ClosePrice { get; set; }
}

public class TransactionViewModel
{
    /// <summary>
    /// 股票代碼
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// 股票名稱
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 成交數量
    /// </summary>
    public decimal TradeQty { get; set; }
    /// <summary>
    /// 成交價格
    /// </summary>
    public decimal TradePrice { get; set; }
    /// <summary>
    /// 成交日期
    /// </summary>
    public DateTime TradeDate { get; set; }

    public TransactionType ActionType { get; set; }
}