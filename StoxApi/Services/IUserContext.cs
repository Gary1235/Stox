using System.Security.Claims;

/// <summary>
/// 取得當前登入使用者資訊的 Context
/// </summary>
public interface IUserContext
{
    public bool IsAuthenticated { get; }
    public Guid UserId { get; }  // 保證回傳 Guid
    public string? Account { get; }
    public string? UserName { get; }
    public string? Role { get; }
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // 保證回傳 Guid，否則拋出例外
    public Guid UserId
    {
        get
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserId");

            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }

            // 找不到或無法轉型時，直接拋出錯誤，防止意外寫入錯誤資料到資料庫
            throw new UnauthorizedAccessException("無法取得使用者的 UserId，請確認 Token 是否有效或包含 UserId Claim。");

            // 備註：如果不希望拋出例外，可以改為回傳 Guid.Empty
            // return Guid.Empty; 
        }
    }

    public string? Account =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("Account");

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("UserName");

    public string? Role =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
}