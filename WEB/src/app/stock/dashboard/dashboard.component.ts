import { Component, computed, signal, TemplateRef, ViewChild, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ChartData, PieChartComponent } from 'src/app/shared/components/pie-chart/pie-chart.component';
import { Column, PagedResult, SharedListComponent } from 'src/app/shared/components/shared-list/shared-list.component';
import { PortfolioSummary, StockSearchViewModel, StockViewModel, TransactionType, TransactionViewModel } from '@features/dashboard/models/stock.model';
import { StockApiService } from '@features/dashboard/services/stock.api';
import { ModalComponent } from '@components/modal/modal.component';
import { TransactionFormComponent } from '../transaction-form/transaction-form.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PieChartComponent, SharedListComponent, CurrencyPipe, ModalComponent, TransactionFormComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private stockApiService = inject(StockApiService);

  detailColumns: Column[] = [];

  // 持倉明細列表
  rowData: PagedResult = new PagedResult();

  // 合計總覽
  portfolio: PortfolioSummary = {} as PortfolioSummary;

  // 💡 存放股票列表的 Signal
  stockListSignal = signal<StockViewModel[]>([]);

  // 💡 Modal 控制與表單狀態變數
  isTradeModalOpen: boolean = false;
  modalTitle: string = '';
  selectedStockFormDefaults: StockViewModel | null = null;

  actionType = signal<TransactionType>(TransactionType.Buy);

  constructor() {}

  ngOnInit() {
    this.setColumns();
    this.getStockList();
  }

  setColumns() {
    this.detailColumns = [
      { key: 'code', label: '代碼' }, // 沒寫 type 預設就是純文字 ('text')
      { key: 'name', label: '股票名稱' },
      { key: 'quantity', label: '持有股數' },
      { key: 'avgCost', label: '平均成本', type: 'currency', pipeArgs: 'USD' },
      { key: 'closePrice', label: '最新收盤價', type: 'currency', pipeArgs: 'USD' },
      { key: 'totalCost', label: '總成本', type: 'currency', pipeArgs: 'USD' },
      { key: 'currentValue', label: '總市價', type: 'currency', pipeArgs: 'USD' },
      { key: 'gainLoss', label: '損益', type: 'currency', pipeArgs: 'USD' },
      {
        key: 'action',
        label: '動作',
        type: 'action',
        actions: [
          {
            label: '買入',
            actionKey: 'buy',
            icon: 'icon-up',
            btnClass: 'btn btn-soft buy',
          },
          {
            label: '賣出',
            actionKey: 'sell',
            icon: 'icon-down',
            btnClass: 'btn btn-soft sell',
          },
        ],
      },
    ];
  }

  getStockList() {
    const searchParams: StockSearchViewModel = {
      keyword: null,
      page: 1,
    };

    this.stockApiService.getStockList(searchParams).subscribe({
      next: (list) => {
        const mappedList = list.map((item) => {
          const totalCost = item.quantity * item.avgCost;
          const currentValue = item.quantity * item.closePrice;
          const gainLoss = currentValue - totalCost;

          return {
            ...item,
            totalCost,
            currentValue,
            gainLoss,
          };
        });

        this.rowData.list = mappedList;
        this.stockListSignal.set(mappedList);
        this.calculatePortfolio(mappedList);
      },
      error: (err) => {
        console.error('取得股票列表失敗', err);
      },
    });
  }

  // 接收來自子元件 (表格) 的按鈕點擊事件
  handleTableAction(event: { actionKey: string; item: any }) {
    const { actionKey, item } = event;

    switch (actionKey) {
      case 'buy':
        this.actionType.set(TransactionType.Buy);
        this.onBuy(item); // 呼叫你原本寫好的買入邏輯
        break;
      case 'sell':
        this.actionType.set(TransactionType.Sell);
        this.onSell(item); // 呼叫你原本寫好的賣出邏輯
        break;
      default:
        console.warn('未知的操作:', actionKey);
        break;
    }
  }

  private calculatePortfolio(list: StockViewModel[]) {
    this.portfolio.holdingCount = list.length;
    this.portfolio.totalValue = list.reduce((sum, item) => sum + (item.currentValue || 0), 0);
    this.portfolio.totalGainLoss = list.reduce((sum, item) => sum + (item.gainLoss || 0), 0);
  }

  chartData = computed<ChartData[]>(() => {
    return this.stockListSignal().map((item) => ({
      label: item.name || '未知',
      value: item.totalCost || 0,
    }));
  });

  // ==========================================
  // 💡 Modal 與交易表單相關邏輯
  // ==========================================

  /**
   * 點擊上方「+ 新增交易」按鈕
   */
  openAddTransactionModal() {
    this.modalTitle = '新增交易錄入';
    this.selectedStockFormDefaults = null; // 確保清空預設值，呈現空白表單
    this.isTradeModalOpen = true;
  }

  /**
   * 點擊列表中的「買入」按鈕
   */
  onBuy(item: StockViewModel) {
    this.modalTitle = `買入 ${item.name} (${item.code})`;
    this.selectedStockFormDefaults = item; // 將該列資料傳入表單作為預設值
    this.isTradeModalOpen = true;
  }

  /**
   * 點擊列表中的「賣出」按鈕
   */
  onSell(item: StockViewModel) {
    this.modalTitle = `賣出 ${item.name} (${item.code})`;
    this.selectedStockFormDefaults = item; // 將該列資料傳入表單作為預設值
    this.isTradeModalOpen = true;
  }

  /**
   * 當表單點擊「確認提交」時觸發
   */
  handleTransactionSubmit(transaction: TransactionViewModel) {
    // 依據表單回傳的 actionType 決定呼叫哪支 API
    const apiCall$ = transaction.actionType === TransactionType.Buy ? this.stockApiService.buyStock(transaction) : this.stockApiService.sellStock(transaction);

    apiCall$.subscribe({
      next: (res) => {
        alert('🎉 交易執行成功！');
        this.closeTradeModal(); // 關閉 Modal
        this.getStockList(); // 重新取得最新列表與總覽數據
      },
      error: (err) => {
        console.error('交易失敗', err);
        alert('交易提交失敗，請檢查網路或輸入資料。');
      },
    });
  }

  /**
   * 關閉 Modal
   */
  closeTradeModal() {
    this.isTradeModalOpen = false;
  }
}
