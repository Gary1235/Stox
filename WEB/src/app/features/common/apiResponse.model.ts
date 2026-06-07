export interface ApiResponse<T = any> {
  /** 執行結果是否成功 */
  success: boolean;
  
  /** 業務狀態碼（可以是 HTTP 狀態碼或自定義代碼） */
  code: number;
  
  /** 回傳訊息（成功提示或錯誤原因） */
  message: string;
  
  /** 實際的資料內容 */
  data: T | null;
  
  /** API 回傳的時間戳記 */
  timestamp: string;
}