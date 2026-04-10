import { Component, computed, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

// shared-list.component.ts (最上方)
export type ColumnType = 'text' | 'seq' | 'currency' | 'date' | 'action' | 'select' | 'checkbox' | 'radio';

export interface ColumnAction {
  label: string;
  actionKey: string; // 給父層辨識按了哪個按鈕 (e.g., 'buy', 'sell')
  icon?: string;     // icon class
  btnClass?: string; // 按鈕的 class
}

export interface ColumnOption {
  label: string;
  value: any;
}

export interface Column {
  key: string;
  label: string;
  type?: ColumnType;          // 預設不填就是 'text'
  actions?: ColumnAction[];   // 當 type 為 'action' 時使用
  options?: ColumnOption[];   // 當 type 為 'select' 或 'radio' 時使用
  pipeArgs?: string;          // 給 date 或 currency pipe 用的參數 (選填)
}

export interface SearchPayload {
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: 'asc' | 'desc';
  keyword?: string;
}

export class PagedResult {
  list: any[] = [];
  totalCount: number = 0;
}

@Component({
  selector: 'comp-shared-list',
  imports: [FormsModule, CommonModule],
  templateUrl: './shared-list.component.html',
  styleUrls: ['./shared-list.component.scss'],
})
export class SharedListComponent {
  Math = Math;

  // --- Signals State ---
  rowData = input<PagedResult>(new PagedResult());
  columns = input<Column[]>([]); // 給予預設空陣列
  searchQuery = signal<string>('');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [5, 10, 20];

  currentSort = signal<{ col: string; direction: 'asc' | 'desc' }>({
    col: 'id',
    direction: 'asc',
  });

  // --- Outputs (將元件內的操作發送給父層) ---
  actionClick = output<{ actionKey: string; item: any }>();
  valueChange = output<{ colKey: string; item: any; newValue: any }>();

  // --- Computed Signals ---
  filteredData = computed(() => {
    let data = [...this.rowData().list];
    const query = this.searchQuery().toLowerCase().trim();
    const sort = this.currentSort();

    // 通用搜尋邏輯 (不再寫死 name, role, department)
    if (query) {
      data = data.filter((item) => {
        return Object.values(item).some(
          (val) => val !== null && val !== undefined && String(val).toLowerCase().includes(query)
        );
      });
    }

    if (sort.col) {
      data.sort((a, b) => {
        const valA = a[sort.col];
        const valB = b[sort.col];

        if (typeof valA === 'string' && typeof valB === 'string') {
          return sort.direction === 'asc' ? valA.localeCompare(valB) : valB.localeCompare(valA);
        } else {
          return sort.direction === 'asc' ? (valA as number) - (valB as number) : (valB as number) - (valA as number);
        }
      });
    }

    return data;
  });

  totalPages = computed(() => {
    const total = this.rowData().totalCount;
    return total === 0 ? 1 : Math.ceil(total / this.pageSize());
  });

  totalPagesArray = computed(() => {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  });

  // --- Methods ---
  sort(columnKey: string) {
    if (typeof columnKey === 'string') {
      const current = this.currentSort();
      if (current.col === columnKey) {
        this.currentSort.set({
          col: columnKey,
          direction: current.direction === 'asc' ? 'desc' : 'asc',
        });
      } else {
        this.currentSort.set({ col: columnKey, direction: 'asc' });
      }
      this.currentPage.set(1);
    }
  }

  getSortIcon(columnKey: string): string {
    if (typeof columnKey === 'string') {
      const sort = this.currentSort();
      if (sort.col !== columnKey) return '↕';
      return sort.direction === 'asc' ? '↑' : '↓';
    }
    return '↕';
  }

  setPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  updatePageSize(newSize: number) {
    this.pageSize.set(newSize);
    this.currentPage.set(1);
  }

  // 處理按鈕點擊
  onAction(actionKey: string, item: any) {
    this.actionClick.emit({ actionKey, item });
  }

  // 處理 input, select, checkbox, radio 值改變
  onModelChange(colKey: string, item: any, newValue: any) {
    this.valueChange.emit({ colKey, item, newValue });
  }
}