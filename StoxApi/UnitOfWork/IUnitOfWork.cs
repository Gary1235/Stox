using System.Transactions;
using Microsoft.EntityFrameworkCore;
using StoxApi.Models.Stox;

public interface IUnitOfWork : IDisposable
{
    public IRepository<T> GetRepository<T>() where T : class;

    public SaveChangeResult SaveChanges();

    public Task<SaveChangeResult> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, DbContext> _contexts;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

    public UnitOfWork(StoxContext stoxContext)
    {
        _contexts = new Dictionary<Type, DbContext>
        {
            { typeof(StoxContext), stoxContext },
        };
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out var repository))
        {
            var targetContext = _contexts.Values.FirstOrDefault(ctx =>
            {
                var props = ctx.GetType().GetProperties();
                return props.Any(p => p.PropertyType == typeof(DbSet<T>));
            });

            if (targetContext == null)
            {
                throw new InvalidOperationException($"No DbContext contains DbSet<{type.Name}>");
            }

            var newRepo = new Repository<T>(targetContext);
            _repositories[type] = newRepo;
        }

        return (IRepository<T>)_repositories[type];
    }

    public SaveChangeResult SaveChanges()
    {
        try
        {
            // 1. 使用 TransactionScope 包裝，確保所有 Context 要嘛全成功，要嘛全失敗
            using (var scope = new TransactionScope())
            {
                foreach (var context in _contexts.Values.Distinct())
                {
                    context.SaveChanges();
                }

                // 2. 所有迴圈內的 SaveChanges 都沒報錯，才正式提交交易
                scope.Complete();
            }

            return SaveChangeResult.Success();
        }
        catch (DbUpdateException ex)
        {
            // 3. 把錯誤訊息往外傳，千萬不要留空字串
            // 實務上建議在這裡加上 Logger 記錄完整 StackTrace
            return SaveChangeResult.Failure($"資料庫更新失敗: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (DbEntityValidationException ex)
        {
            // EF6 特有的驗證錯誤
            return SaveChangeResult.Failure($"資料驗證失敗: {ex.Message}");
        }
        catch (Exception ex)
        {
            // 加上最基底的 Exception，防止其他意外錯誤（例如網路斷線、資料庫連線失敗）讓系統掛掉
            return SaveChangeResult.Failure($"發生未預期的系統錯誤: {ex.Message}");
        }
    }

    public async Task<SaveChangeResult> SaveChangesAsync()
    {
        try
        {
            var contexts = _contexts.Values.Distinct().ToList();

            // 使用 TransactionScope 包裝，確保跨 Context 的資料一致性
            // 注意：必須加上 TransactionScopeAsyncFlowOption.Enabled 才能支援 async/await
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 如果 Context 彼此獨立，可以使用 Task.WhenAll 平行處理提升效能
                var saveTasks = contexts.Select(c => c.SaveChangesAsync());
                await Task.WhenAll(saveTasks);

                // 所有任務都沒拋出異常，才提交交易
                scope.Complete();
            }

            return SaveChangeResult.Success();
        }
        catch (DbUpdateException ex)
        {
            // 務必將錯誤訊息傳出去，或是記錄在 Logger 裡
            // _logger.LogError(ex, "資料庫更新失敗");
            return SaveChangeResult.Failure($"更新資料庫時發生錯誤: {ex.Message}");
        }
        catch (DbEntityValidationException ex) // 註：此例外通常存在於 EF6，EF Core 已棄用
        {
            // 若是 EF6，建議將 EntityValidationErrors 轉為字串回傳，否則很難查是哪個欄位驗證失敗
            return SaveChangeResult.Failure($"資料驗證失敗: {ex.Message}");
        }
        catch (Exception ex)
        {
            // 建議加上最基底的 Exception，防止未知的錯誤讓系統崩潰
            return SaveChangeResult.Failure($"發生未預期的系統錯誤: {ex.Message}");
        }
    }

    public void Dispose()
    {
        foreach (var context in _contexts.Values.Distinct())
        {
            context.Dispose();
        }
    }
}

[Serializable]
internal class DbEntityValidationException : Exception
{
    public DbEntityValidationException()
    {
    }

    public DbEntityValidationException(string? message) : base(message)
    {
    }

    public DbEntityValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}