using Hangfire;

public static class HangfireJobScheduler
{
    // 擴充 IApplicationBuilder
    public static void UseMyHangfireJobs(this IApplicationBuilder app)
    {
        // 取得時區 (台灣時間)
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

        // 註冊你的定期任務
        // 注意這裡的寫法：泛型傳入介面，Hangfire 會自動從 DI 容器把實作類別解析出來
        RecurringJob.AddOrUpdate<IPortfolioService>(
            "Daily-Report-Job", // 任務的唯一 ID
            service => service.GetAllStockQuoteAsync(), // 呼叫的方法
            "0 8 * * *", // Cron 表達式：每天早上 8 點
            new RecurringJobOptions { TimeZone = tz } 
        );

        // 如果有其他任務，就繼續加在下面...
        // RecurringJob.AddOrUpdate<IOtherService>(...);
    }
}