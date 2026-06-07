import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { TransactionViewModel } from '@features/dashboard/models/stock.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TransactionApiService {
  private http = inject(HttpClient);

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
