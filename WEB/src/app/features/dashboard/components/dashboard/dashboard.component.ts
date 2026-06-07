import { Component, computed, signal, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { interval, startWith, Subject, switchMap, takeUntil } from 'rxjs';
import { ChartData, PieChartComponent } from 'src/app/shared/components/pie-chart/pie-chart.component';
import { Column, PagedResult, SharedListComponent } from 'src/app/shared/components/shared-list/shared-list.component';
import { PortfolioSummary, StockOptionViewModel, StockQuoteViewModel, StockSearchViewModel, StockViewModel, TransactionType, TransactionViewModel } from '@features/dashboard/models/stock.model';
import { StockApiService } from '@features/dashboard/services/stock.api';
import { ModalComponent } from '@components/modal/modal.component';
import { TransactionFormComponent } from '../../../transaction/components/transaction-form/transaction-form.component';
import { StockParamService } from '@features/dashboard/services/stock.param';
import { TransactionApiService } from '@features/transaction/services/transaction.api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PieChartComponent, SharedListComponent, CurrencyPipe, ModalComponent, TransactionFormComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private stockApiService = inject(StockApiService);
  private transactionApiService = inject(TransactionApiService);
  private stockParam = inject(StockParamService);
  private destroy$ = new Subject<void>();
  private quoteRefreshIntervalMs = 10000;

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

  lastQuoteUpdate = signal<string>('尚未更新');

  realCapital = signal<number>(0);

  constructor() {}

  ngOnInit() {
    this.setColumns();
    this.getStockList();
    this.getTotalRealCapital();
    this.startQuotePolling();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
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

  private startQuotePolling() {
    interval(this.quoteRefreshIntervalMs)
      .pipe(
        startWith(0),
        switchMap(() => this.stockApiService.getStockQuote()),
        takeUntil(this.destroy$),
      )
      .subscribe({
        next: (quotes) => this.applyLiveQuotes(quotes),
        error: (err) => {
          console.error('取得即時報價失敗', err);
        },
      });
  }

  private applyLiveQuotes(quotes: StockQuoteViewModel[]) {
    const currentList = this.stockListSignal();
    if (!currentList.length) {
      return;
    }

    const quoteMap = new Map<string, number>();
    quotes.forEach((quote) => {
      if (quote.symbol && quote.livePrice != null) {
        quoteMap.set(quote.symbol, quote.livePrice);
      }
    });

    const updatedList = currentList.map((item) => {
      const livePrice = item.code ? quoteMap.get(item.code) : undefined;
      if (livePrice == null) {
        return item;
      }

      const totalCost = item.quantity * item.avgCost;
      const currentValue = item.quantity * livePrice;
      const gainLoss = currentValue - totalCost;

      return {
        ...item,
        closePrice: livePrice,
        totalCost,
        currentValue,
        gainLoss,
      };
    });

    this.rowData.list = updatedList;
    this.stockListSignal.set(updatedList);
    this.calculatePortfolio(updatedList);
    this.lastQuoteUpdate.set(new Date().toLocaleTimeString());
  }

  getTotalRealCapital() {
    this.stockApiService.getTotalRealCapital().subscribe(total => {
      this.realCapital.set(total);
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
    const apiCall$ = transaction.actionType === TransactionType.Buy ? this.transactionApiService.buyStock(transaction) : this.transactionApiService.sellStock(transaction);

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
