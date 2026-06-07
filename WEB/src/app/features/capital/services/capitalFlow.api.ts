import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse, CapitalFlowSearchViewModel, CapitalFlowViewModel, CapitalSummaryViewModel } from '../models/capitalFlow.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CapitalFlowApiService {
  private http = inject(HttpClient);

  /**
   * 1. 取得現金流列表 (排除 DeleteAt)
   */
  getList(search: CapitalFlowSearchViewModel): Observable<CapitalFlowViewModel[]> {
    return this.http.post<CapitalFlowViewModel[]>('CapitalFlow/search', search);
  }

  /**
   * 2. 新增存入本金
   */
  createDeposit(model: CapitalFlowViewModel): Observable<ApiResponse> {
    return this.http.post<ApiResponse>('CapitalFlow/deposit', model);
  }

  /**
   * 3. 新增提領本金
   */
  createWithdraw(model: CapitalFlowViewModel): Observable<ApiResponse> {
    return this.http.post<ApiResponse>('CapitalFlow/withdraw', model);
  }

  /**
   * 4. 軟刪除
   */
  softDelete(id: string): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`CapitalFlow/delete/${id}`);
  }

  /**
   * 5. 取得投入本金總覽 (TWD 加總與最後交易日)
   */
  getSummary(userId: string): Observable<CapitalSummaryViewModel> {
    return this.http.get<CapitalSummaryViewModel>(`CapitalFlow/summary/${userId}`);
  }
}
