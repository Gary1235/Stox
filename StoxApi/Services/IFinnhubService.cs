using System.Net.Http.Json;
using System.Text.Json.Serialization;

public interface IFinnhubService
{
    /// <summary>
    /// 取得 即時價格
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public Task<StockQuote?> GetQuoteAsync(string? symbol);
}
public class FinnhubService : IFinnhubService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = ""; // 建議從環境變數或 User Secrets 讀取

    public FinnhubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://finnhub.io/api/v1/");
        // 將 API Key 放進 Header，程式碼更整潔
        _httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", ApiKey);
    }

    /// <summary>
    /// 取得 即時價格
    /// </summary>
    /// <param name="symbol">股票代號</param>
    /// <returns></returns>
    public async Task<StockQuote?> GetQuoteAsync(string? symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return null;
        }

        // 直接使用 .NET 內建的 GetFromJsonAsync
        return await _httpClient.GetFromJsonAsync<StockQuote>($"quote?symbol={symbol}");
    }
}

// 定義回傳資料模型 (根據 Finnhub API 文件)
