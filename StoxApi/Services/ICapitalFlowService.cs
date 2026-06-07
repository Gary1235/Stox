using Microsoft.EntityFrameworkCore;
using StoxApi.Models.Stox;

public interface ICapitalFlowService
{
    /// <summary>
    /// 取得投入本金總覽 (加總 TWD 投入與提領)
    /// </summary>
    public Task<CapitalSummaryViewModel> GetSummaryAsync(Guid userId);
    /// <summary>
    /// 取得現金流列表（排除已軟刪除資料）
    /// </summary>
    public Task<List<CapitalFlowViewModel>> GetListAsync(CapitalFlowSearchViewModel search);
    /// <summary>
    /// 新增 存入本金 (Deposit)
    /// </summary>
    public Task<SaveChangeResult<Guid>> CreateDepositAsync(CapitalFlowViewModel model);
    /// <summary>
    /// 新增 提領本金 (Withdraw)
    /// </summary>
    public Task<SaveChangeResult<Guid>> CreateWithdrawAsync(CapitalFlowViewModel model);
    /// <summary>
    /// 軟刪除 (Soft Delete)
    /// </summary>
    public Task<SaveChangeResult> SoftDeleteAsync(Guid id);
}

public class CapitalFlowService : ICapitalFlowService
{
    private readonly StoxContext _stox;
    private readonly IUserContext _userContext;

    public CapitalFlowService(StoxContext stox, IUserContext userContext)
    {
        _stox = stox;
        _userContext = userContext;
    }

    public async Task<CapitalSummaryViewModel> GetSummaryAsync(Guid userId)
    {
        // 抓取該用戶所有未刪除的 TWD 資料
        var flows = await _stox.CapitalFlow
            .Where(c => c.UserId == userId && c.DeleteAt == null && c.Currency == "TWD")
            .ToListAsync();

        if (!flows.Any())
        {
            return new CapitalSummaryViewModel
            {
                TotalTwdCapital = 0,
                LastTransactionDate = DateTime.MinValue
            };
        }

        // 計算總額：投入(1)為正，提領(2)為負
        decimal total = flows.Sum(c => c.FlowType == (int)CapitalFlowType.Deposit ? c.Amount :
            c.FlowType == (int)CapitalFlowType.Withdrawal ? -c.Amount : 0);

        DateTime lastDate = flows.Max(c => c.TransactionDate);

        return new CapitalSummaryViewModel
        {
            TotalTwdCapital = total,
            LastTransactionDate = lastDate
        };
    }

    public async Task<List<CapitalFlowViewModel>> GetListAsync(CapitalFlowSearchViewModel search)
    {
        // 基本篩選：排除已軟刪除，並限制在時間區間內
        var query = _stox.CapitalFlow
        .AsNoTracking()
        .Include(x => x.User)
        .Where(c => c.DeleteAt == null && c.TransactionDate >= search.StartDate && c.TransactionDate <= search.EndDate);

        // 如果搜尋條件不是「兩者」，則加入 FlowType 篩選
        if (search.FlowType != CapitalFlowType.Both)
        {
            query = query.Where(c => c.FlowType == (int)search.FlowType);
        }

        // 映射至 ViewModel
        return await query
            .OrderByDescending(c => c.TransactionDate)
            .Select(c => new CapitalFlowViewModel
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.UserName : string.Empty, // 假設 User Entity 有 Name 欄位
                FlowType = (CapitalFlowType)c.FlowType,
                Amount = c.Amount,
                Currency = c.Currency,
                TransactionDate = c.TransactionDate,
                Note = c.Note
            })
            .ToListAsync();
    }

    public async Task<SaveChangeResult<Guid>> CreateDepositAsync(CapitalFlowViewModel model)
    {
        var entity = new CapitalFlow
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            FlowType = (int)CapitalFlowType.Deposit, // 1
            Amount = model.Amount,
            Currency = model.Currency ?? "TWD",
            TransactionDate = model.TransactionDate,
            Note = model.Note,
            CreatedAt = DateTime.UtcNow,
            CreatedUserId = _userContext.UserId // 執行操作的人
        };

        _stox.CapitalFlow.Add(entity);

        var result = await _stox.SaveChangesAsync();

        return result > 0
            ? SaveChangeResult<Guid>.Success(entity.Id)
            : SaveChangeResult<Guid>.Failure("新增存入本金失敗");
    }

    public async Task<SaveChangeResult<Guid>> CreateWithdrawAsync(CapitalFlowViewModel model)
    {
        var entity = new CapitalFlow
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            FlowType = (int)CapitalFlowType.Withdrawal, // 2
            Amount = model.Amount,
            Currency = model.Currency ?? "TWD",
            TransactionDate = model.TransactionDate,
            Note = model.Note,
            CreatedAt = DateTime.UtcNow,
            CreatedUserId = _userContext.UserId // 執行操作的人
        };

        _stox.CapitalFlow.Add(entity);

        var result = await _stox.SaveChangesAsync();

        return result > 0
            ? SaveChangeResult<Guid>.Success(entity.Id)
            : SaveChangeResult<Guid>.Failure("新增提領本金失敗");
    }

    public async Task<SaveChangeResult> SoftDeleteAsync(Guid id)
    {
        var entity = await _stox.CapitalFlow
            .FirstOrDefaultAsync(c => c.Id == id && c.DeleteAt == null);

        if (entity == null)
        {
            return SaveChangeResult.Failure("未找到要刪除的資料");
        }


        // 標記刪除時間，不實際刪除資料
        entity.DeleteAt = DateTime.UtcNow;

        var result = await _stox.SaveChangesAsync();

        return result > 0
            ? SaveChangeResult.Success()
            : SaveChangeResult.Failure("軟刪除失敗");
    }
}