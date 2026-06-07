import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CapitalFlowSearchViewModel, CapitalFlowType, CapitalFlowViewModel, CapitalSummaryViewModel } from '@features/capital/models/capitalFlow.model';
import { CapitalFlowApiService } from '@features/capital/services/capitalFlow.api';
import { AuthService } from '@services/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-capital-flow',
  imports: [DecimalPipe, FormsModule, DatePipe, CommonModule],
  templateUrl: './capital-flow.component.html',
  styleUrl: './capital-flow.component.scss',
})
export class CapitalFlowComponent {
  private apiService = inject(CapitalFlowApiService);
  private authService = inject(AuthService);

  // 將 Enum 暴露給 Template 使用
  protected readonly CapitalFlowType = CapitalFlowType;

  // 使用者資訊與基本狀態 Signal
  userId = this.authService.currentUser()?.userId ?? ''; // 實務上可從 AuthService 取得
  list = signal<CapitalFlowViewModel[]>([]);
  summary = signal<CapitalSummaryViewModel | null>(null);

  // 搜尋與過濾條件 Signal
  currentFlowType = signal<CapitalFlowType>(CapitalFlowType.Both);
  searchKeyword = signal<string>('');

  // Modal 開關控制 Signal (綁定至 HTML Checkbox)
  isModalOpen = signal<boolean>(false);

  // 新增表單的欄位 Signal
  formFlowType = signal<CapitalFlowType>(CapitalFlowType.Withdraw);
  formDate = signal<string>(new Date().toISOString().split('T')[0]);
  formAmount = signal<number>(0);
  formCategory = signal<string>('生活支出');
  formNote = signal<string>('');

  // 透過 computed 動態計算三大 KPI (不影響原本 API 的 summary 邏輯)
  totalInflow = computed(() =>
    this.list()
      .filter((item) => item.flowType === CapitalFlowType.Deposit)
      .reduce((sum, item) => sum + item.amount, 0),
  );

  totalOutflow = computed(() =>
    this.list()
      .filter((item) => item.flowType === CapitalFlowType.Withdraw)
      .reduce((sum, item) => sum + item.amount, 0),
  );

  netCashFlow = computed(() => this.totalInflow() - this.totalOutflow());

  // 前端即時關鍵字過濾 (搜尋 類別 或 備註)
  filteredList = computed(() => {
    const keyword = this.searchKeyword().toLowerCase().trim();
    if (!keyword) return this.list();
    return this.list().filter((item) => item.note?.toLowerCase().includes(keyword));
  });

  ngOnInit(): void {
    this.loadData();
    this.loadSummary();
  }

  /**
   * 載入現金流列表
   */
  loadData(): void {
    // 預設查詢前後各一年的範圍，可依需求調整
    const searchModel: CapitalFlowSearchViewModel = {
      startDate: new Date(new Date().getFullYear() - 1, 0, 1).toISOString(),
      endDate: new Date(new Date().getFullYear() + 1, 11, 31).toISOString(),
      flowType: this.currentFlowType(),
    };

    this.apiService.getList(searchModel).subscribe({
      next: (data) => this.list.set(data),
      error: (err) => console.error('取得列表失敗:', err),
    });
  }

  /**
   * 載入本金總覽摘要
   */
  loadSummary(): void {
    this.apiService.getSummary(this.userId).subscribe({
      next: (data) => this.summary.set(data),
      error: (err) => console.error('取得摘要失敗:', err),
    });
  }

  /**
   * 切換全部 / 流入 / 流出 頁籤
   */
  setFlowType(type: CapitalFlowType): void {
    this.currentFlowType.set(type);
    this.loadData();
  }

  /**
   * 刪除明細紀錄
   */
  deleteItem(id?: string): void {
    if (!id) return;
    if (confirm('確定要刪除此筆記錄嗎？')) {
      this.apiService.softDelete(id).subscribe({
        next: () => {
          this.loadData();
          this.loadSummary();
        },
        error: (err) => console.error('刪除失敗:', err),
      });
    }
  }

  /**
   * 送出表單 (新增明細)
   */
  submitForm(): void {
    if (this.formAmount() <= 0) {
      alert('請輸入有效金額！');
      return;
    }

    // 將「類別」與「備註描述」整合存入 note 欄位
    const combinedNote = `[${this.formCategory()}] ${this.formNote()}`.trim();

    const model: CapitalFlowViewModel = {
      userId: this.userId,
      flowType: this.formFlowType(),
      amount: this.formAmount(),
      transactionDate: new Date(this.formDate()).toISOString(),
      note: combinedNote,
    };

    const request$: Observable<any> = this.formFlowType() === CapitalFlowType.Deposit ? this.apiService.createDeposit(model) : this.apiService.createWithdraw(model);

    request$.subscribe({
      next: () => {
        this.loadData();
        this.loadSummary();
        this.resetForm();
        this.isModalOpen.set(false); // 成功後關閉 Modal
      },
      error: (err) => console.error('新增失敗:', err),
    });
  }

  /**
   * 重設表單欄位
   */
  private resetForm(): void {
    this.formFlowType.set(CapitalFlowType.Withdraw);
    this.formDate.set(new Date().toISOString().split('T')[0]);
    this.formAmount.set(0);
    this.formCategory.set('生活支出');
    this.formNote.set('');
  }

  /**
   * 解析顯示於表格的類別名稱
   */
  parseCategory(note?: string): string {
    if (!note) return '一般';
    const match = note.match(/^\[(.*?)\]/);
    return match ? match[1] : '一般';
  }

  /**
   * 解析顯示於表格的純備註說明
   */
  parseNoteContent(note?: string): string {
    if (!note) return '';
    return note.replace(/^\[.*?\]\s*/, '');
  }
}
