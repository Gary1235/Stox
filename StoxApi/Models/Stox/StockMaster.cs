using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class StockMaster
{
    public Guid Id { get; set; }

    /// <summary>
    /// 股票代號
    /// </summary>
    public string Symbol { get; set; } = null!;

    /// <summary>
    /// 股票名稱
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 市場別 (例: &apos;US&apos;, &apos;TW&apos;，用來區分台股或美股)
    /// </summary>
    public string Market { get; set; } = null!;

    /// <summary>
    /// 資產類型 (例: &apos;Stock&apos; 個股, &apos;ETF&apos; 指數型基金)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 計價幣別 (例: &apos;USD&apos;, &apos;TWD&apos;，計算投資組合總值時必備)
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// 產業別 (例: &apos;Technology&apos;, &apos;Semiconductors&apos;，方便做產業分佈圓餅圖)
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// 隱藏的搜尋關鍵字 (例: &apos;台積電,tsmc,護國神山,2330&apos;)
    /// </summary>
    public string? SearchKeywords { get; set; }

    /// <summary>
    /// 權重排序 (熱門股如 NVDA, TSLA, 0050 可以設高一點，下拉選單會排前面)
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 狀態 (1=正常交易, 0=已下市。下市不刪除資料，以免歷史交易紀錄關聯報錯)
    /// </summary>
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
