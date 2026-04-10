public interface ISearchCommon
{
    /// <summary>
    /// 關鍵字
    /// </summary>
    public string? Keyword { get; set; }
    /// <summary>
    /// 指定頁數
    /// </summary>
    public int Page { get; set; }
}

public class SaveChangeResult
{
    public bool IsSuccess { get; protected set; } // 設為 protected，強迫使用工廠方法
    public string? Message { get; protected set; }

    // 靜態工廠方法
    public static SaveChangeResult Success(string? msg = null)
        => new SaveChangeResult { IsSuccess = true, Message = msg };

    public static SaveChangeResult Failure(string msg)
        => new SaveChangeResult { IsSuccess = false, Message = msg };

    public static SaveChangeResult<T> Success<T>(T data, string? msg = null)
        => new SaveChangeResult<T> { IsSuccess = true, Data = data, Message = msg };

    public static SaveChangeResult<T> Failure<T>(string msg)
        => new SaveChangeResult<T> { IsSuccess = false, Message = msg };
}

public class SaveChangeResult<T> : SaveChangeResult
{
    public T? Data { get; internal set; }
}