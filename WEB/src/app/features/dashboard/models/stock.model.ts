/**
 * 持倉明細 模型
 */
export interface StockViewModel {
  /** 股票代碼 */
  code: string | null;
  /** 股票名稱 */
  name: string | null;
  /** 持有股數 */
  quantity: number;
  /** 平均成本價格 */
  avgCost: number;
  /** 最新收盤價格 (對應 C# 的 ClosePrice) */
  closePrice: number;

  // ⚠️ 提醒：Interface 不能包含 get() 計算邏輯。
  // 如果這些值是前端自己要算的，你可以將它們定義為可選屬性 (?)
  // 並在取得 API 資料後，透過 Array.map() 去手動計算賦值。
  /** 總成本 */
  totalCost?: number;
  /** 總市價 */
  currentValue?: number;
  /** 損益 */
  gainLoss?: number;
}

/**
 * 投資組合總攬 模型
 */
export interface PortfolioSummary {
  /** 總市值 */
  totalValue: number;
  /** 總損益 */
  totalGainLoss: number;
  /** 現金水位 / 持有檔數 */
  holdingCount: number;
}

/**
 * 股票搜尋 模型
 * 對應 C# 的 StockSearchViewModel
 */
export interface StockSearchViewModel {
  /** 搜尋關鍵字 */
  keyword: string | null;
  /** 頁碼 (在發送請求時，需確保有給定預設值 1) */
  page: number;
}

/**
 * 交易類型 列舉
 * (Enum 維持原樣即可，因為它本身就是 TypeScript 處理常數的標準寫法)
 * 對應 C# 的 TransactionType
 */
export enum TransactionType {
  /** 買進 */
  Buy = 0,
  /** 賣出 */
  Sell = 1,
}

/**
 * 股票交易 模型
 * 對應 C# 的 TransactionViewModel
 */
export interface TransactionViewModel {
  /** 股票代碼 */
  code: string | null;
  /** 股票名稱 */
  name: string | null;
  /** 成交數量 */
  tradeQty: number;
  /** 成交價格 */
  tradePrice: number;
  /** 成交日期 (通常傳送 ISO 8601 格式字串給後端) */
  tradeDate: string | Date;
  /** 交易行為 (買進/賣出) */
  actionType: TransactionType;
}