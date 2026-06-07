import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StockOptionViewModel, StockQuoteViewModel, StockSearchViewModel, StockViewModel, TransactionViewModel } from '../models/stock.model';

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

  getTotalRealCapital(): Observable<number> {
    return this.http.post<number>('Stock/GetTotalRealCapitalAsync', null);
  }

  getStockOptions(): Observable<StockOptionViewModel[]> {
    return this.http.post<StockOptionViewModel[]>('Stock/GetStockOptions', null);
  }

  getStockQuote(): Observable<StockQuoteViewModel[]> {
    return this.http.post<StockQuoteViewModel[]>('Stock/GetAllStockQuote', null);
  }
}
