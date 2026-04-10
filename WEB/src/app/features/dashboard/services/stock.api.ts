import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StockSearchViewModel, StockViewModel, TransactionViewModel } from '../models/stock.model';

@Injectable({
  providedIn: 'root',
})
export class StockApiService {
  private http = inject(HttpClient);

  constructor() {}

  /**
   * 取得股票列表
   *
   */
  getStockList(searchParams: StockSearchViewModel): Observable<StockViewModel[]> {
    return this.http.post<StockViewModel[]>('Stock/GetStockList', searchParams);
  }

  /**
   * 買入股票
   *
   */
  buyStock(transaction: TransactionViewModel): Observable<any> {
    // 若後端有回傳特定的 Result Model，可以將 any 替換成該 Model 型別
    return this.http.post<any>('Stock/BuyStock', transaction);
  }

  /**
   * 賣出股票
   *
   */
  sellStock(transaction: TransactionViewModel): Observable<any> {
    return this.http.post<any>('Stock/SellStock', transaction);
  }
}
