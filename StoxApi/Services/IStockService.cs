using StoxApi.Models.Stox;

public interface IStockService
{
    /// <summary>
    /// 取出 庫存列表
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public List<StockViewModel> GetStockList(StockSearchViewModel search);
    /// <summary>
    /// 買入
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public Task<SaveChangeResult> BuyStockAsync(TransactionViewModel model);
    /// <summary>
    /// 賣出 (包含手續費、證交稅與已實現損益計算)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public SaveChangeResult SellStock(TransactionViewModel model);
    /// <summary>
    /// 取得 所有已追蹤的股票即時價格
    /// </summary>
    /// <returns></returns>
    public Task<List<StockQuoteViewModel>> GetAllStockQuote();
    /// <summary>
    /// 計入每日股價(開盤、收盤、當日最高/低)
    /// </summary>
    /// <returns></returns>
    public SaveChangeResult RecordDailyPrice();
}

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFinnhubService _finnhubService;

    public StockService(IUnitOfWork unitOfWork, IFinnhubService finnhubService)
    {
        _unitOfWork = unitOfWork;
        _finnhubService = finnhubService;
    }

    public List<StockViewModel> GetStockList(StockSearchViewModel search)
    {
        var list = new List<StockViewModel>();

        var query = _unitOfWork.GetRepository<InventoryLot>().AsNoTracking();

        if (!string.IsNullOrEmpty(search.Keyword))
        {
            var keyword = search.Keyword;
            query = query.Where(x => (x.StockCode != null && x.StockCode.Contains(keyword)) || (x.StockName != null && x.StockName.Contains(keyword)));
        }

        list = query
        .GroupBy(x => new { x.StockCode, x.StockName })
        .Select(x => new
        {
            Code = x.Key.StockCode,
            Name = x.Key.StockName,
            TotalQty = x.Sum(s => s.RemainingQty),
            TotalAmount = x.Sum(s => s.CostPrice * s.RemainingQty),
        })
        .AsEnumerable()
        .Select(x => new StockViewModel
        {
            Code = x.Code,
            Name = x.Name,
            Quantity = x.TotalQty,
            AvgCost = x.TotalQty > 0 ? x.TotalAmount / x.TotalQty : 0,
        })
        .ToList();

        return list;
    }
    /// <summary>
    /// 買入
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<SaveChangeResult> BuyStockAsync(TransactionViewModel model)
    {
        var result = new SaveChangeResult();

        if (string.IsNullOrEmpty(model.Code))
        {
            return result;
        }

        // 寫入買入庫存批次檔
        var inventory = new InventoryLot
        {
            Id = Guid.CreateVersion7(),
            StockCode = model.Code,
            StockName = model.Name,
            BuyDate = model.TradeDate,
            OriginalQty = model.TradeQty,
            RemainingQty = model.TradeQty,
            CostPrice = model.TradePrice,
            CreatedDate = DateTime.Now
        };

        // 寫入交易紀錄
        decimal fee = 0; // todo 待查詢計算方式
        var transaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            StockCode = model.Code,
            StockName = model.Name,
            TradeDate = model.TradeDate,
            ActionType = (int)model.ActionType,
            Quantity = model.TradeQty,
            Price = model.TradePrice,
            Fee = fee,
            TotalAmount = (model.TradeQty * model.TradePrice) + fee,
            CreatedDate = DateTime.Now
        };
        transaction.InventoryLots.Add(inventory);

        _unitOfWork.GetRepository<Transaction>().Add(transaction);
        result = await _unitOfWork.SaveChangesAsync();

        return result;
    }
    /// <summary>
    /// 賣出 (單純計算價差，不包含手續費與規費)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public SaveChangeResult SellStock(TransactionViewModel model)
    {
        var result = new SaveChangeResult();

        // 1. 基本防呆檢核
        if (string.IsNullOrEmpty(model.Code) || model.TradeQty <= 0)
        {
            return SaveChangeResult.Failure("參數錯誤：股票代號不可為空，且交易數量必須大於 0");
        }

        // 2. 取出該股票的剩餘庫存 (FIFO 先進先出)
        var inventories = _unitOfWork.GetRepository<InventoryLot>()
            .Where(x => x.StockCode == model.Code && x.RemainingQty > 0)
            .OrderBy(x => x.BuyDate)
            .ToList();

        // 3. 檢查庫存是否充足 (防止超賣)
        decimal totalRemaining = inventories.Sum(x => x.RemainingQty);
        if (model.TradeQty > totalRemaining)
        {
            return SaveChangeResult.Failure($"庫存餘額不足！欲賣出 {model.TradeQty}，但目前僅剩餘 {totalRemaining}");
        }

        // --- 單純計算總價 ---
        decimal totalSellAmount = model.TradeQty * model.TradePrice;

        // 4. 寫入交易紀錄 (Transaction)
        var transaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            // CreatedUserId = currentUserId, // TODO: 記得補上當前登入使用者的 ID
            StockCode = model.Code,
            StockName = model.Name,
            TradeDate = model.TradeDate,
            ActionType = (int)model.ActionType,
            Quantity = model.TradeQty,
            Price = model.TradePrice,
            Fee = 0, // 不計算手續費
            TotalAmount = totalSellAmount, // 單純 數量 * 單價
            CreatedDate = DateTime.Now
        };
        _unitOfWork.GetRepository<Transaction>().Add(transaction);

        // 5. 異動庫存批次並寫入沖銷明細
        decimal remainingToSell = model.TradeQty;

        foreach (var lot in inventories)
        {
            if (remainingToSell <= 0) break; // 賣夠了就跳出

            decimal actualQty = 0; // 紀錄「這一筆庫存」實際被扣除的數量

            if (remainingToSell >= lot.RemainingQty)
            {
                // 如果要賣的數量 >= 這筆庫存剩餘量，就把這筆庫存清空
                actualQty = lot.RemainingQty;
                remainingToSell -= lot.RemainingQty;
                lot.RemainingQty = 0;
            }
            else
            {
                // 如果要賣的數量 < 這筆庫存剩餘量，只扣除需要賣的部分
                actualQty = remainingToSell;
                lot.RemainingQty -= remainingToSell;
                remainingToSell = 0;
            }

            // --- 已實現損益計算 (純價差) ---
            decimal lotSellAmount = actualQty * model.TradePrice;
            decimal lotCostAmount = actualQty * lot.CostPrice;
            decimal realizedProfit = lotSellAmount - lotCostAmount; // 單純計算：賣出總價 - 買入成本
                                                                    // ----------------------------------------

            // 6. 寫入交易沖銷明細 (TransactionDetail)
            var detail = new TransactionDetail
            {
                Id = Guid.CreateVersion7(),
                Qty = actualQty,
                RealizedProfit = realizedProfit,
                InventoryLot = lot,
                SellTransaction = transaction
            };
            _unitOfWork.GetRepository<TransactionDetail>().Add(detail);
        }

        // 7. 一次性提交所有變更
        result = _unitOfWork.SaveChanges();

        return result;
    }

    /// <summary>
    /// 取得 所有已追蹤的股票即時價格
    /// </summary>
    /// <returns></returns>
    public async Task<List<StockQuoteViewModel>> GetAllStockQuote()
    {
        var stockQuotes = new List<StockQuoteViewModel>();

        var allStockCode = _unitOfWork.GetRepository<InventoryLot>().AsNoTracking()
        .Select(x => x.StockCode)
        .Distinct()
        .ToList();

        foreach (var code in allStockCode)
        {
            var quote = await _finnhubService.GetQuoteAsync(code);

            if (quote != null)
            {
                stockQuotes.Add(new StockQuoteViewModel
                {
                    Code = code,
                    LivePrice = quote.CurrentPrice,
                });
            }
        }

        // todo 通知功能(先做發mail) 必須先建一個table 紀錄要追蹤的價位通知(超過設定高位/低位)

        return stockQuotes;
    }

    /// <summary>
    /// 計入每日股價(開盤、收盤、當日最高/低)
    /// </summary>
    /// <returns></returns>
    public SaveChangeResult RecordDailyPrice()
    {
        // 只記錄有追蹤的股票
        // 只從追蹤開始到取消追蹤這段時間
        // 意義??

        return SaveChangeResult.Failure("此功能需要在想一下細節");
    }
}