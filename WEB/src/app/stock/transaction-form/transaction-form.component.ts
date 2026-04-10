import { Component, Output, EventEmitter, Input, OnInit, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TransactionType, TransactionViewModel, StockViewModel } from '@features/dashboard/models/stock.model';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.scss',
})
export class TransactionFormComponent implements OnInit {
  // 💡 方便在 HTML 中使用 Enum
  transactionType = TransactionType;
  tradeForm!: FormGroup;

  actionType = input<TransactionType>(TransactionType.Buy); // 預設為買入
  // 💡 (選填) 如果是從列表點擊「買入/賣出」，可以帶入預設資料
  @Input() defaultStockData: StockViewModel | null = null;

  // 💡 提交表單或取消時通知父元件
  @Output() submitted = new EventEmitter<TransactionViewModel>();
  @Output() cancelled = new EventEmitter<void>();

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    this.initForm();
  }

  initForm() {
    // 💡 建立表單控制項與驗證規則
    this.tradeForm = this.fb.group({
      actionType: [this.actionType(), Validators.required],
      code: [this.defaultStockData?.code || '', Validators.required],
      name: [this.defaultStockData?.name || ''],
      tradeQty: [this.defaultStockData ? this.defaultStockData.quantity : '', [Validators.required, Validators.min(1)]],
      tradePrice: [this.defaultStockData?.closePrice || '', [Validators.required, Validators.min(0.01)]],
      tradeDate: [new Date().toISOString().substring(0, 10), Validators.required], // 預設今天
    });
  }

  onSubmit() {
    if (this.tradeForm.valid) {
      // 組裝輸出的資料
      const formValue = this.tradeForm.value;
      const result: TransactionViewModel = {
        ...formValue,
        // 確保日期轉回 Date 物件
        tradeDate: new Date(formValue.tradeDate),
      };
      this.submitted.emit(result);
    }
  }

  onCancel() {
    this.cancelled.emit();
  }
}
