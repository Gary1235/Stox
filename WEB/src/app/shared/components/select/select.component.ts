import { OverlayModule } from '@angular/cdk/overlay';
import { Component, computed, ElementRef, forwardRef, input, output, Pipe, PipeTransform, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';


// --- 關鍵字高亮 Pipe ---
@Pipe({
  name: 'highlight',
  standalone: true
})
export class HighlightPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) { }

  transform(text: string, search: string | null): SafeHtml {
    if (!search || !text) return text;
    const regex = new RegExp(`(${search})`, 'gi');
    const highlighted = text.replace(regex, `<span class="highlight-text">$1</span>`);
    return this.sanitizer.bypassSecurityTrustHtml(highlighted);
  }
}

// --- 主要的 Select 元件 ---
export interface SelectOption {
  value: string | number;
  label: string | null;
}

@Component({
  selector: 'app-select',
  imports: [ReactiveFormsModule, OverlayModule, HighlightPipe],
  templateUrl: './select.component.html',
  styleUrl: './select.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ],
})
export class SelectComponent implements ControlValueAccessor {
  options = input<SelectOption[]>([]);
  multiple = input<boolean>(false);
  placeholder = input<string>('請選擇...');
  
  change = output<SelectOption>();

  searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');

  isOpen = signal<boolean>(false);
  searchControl = new FormControl('');

  // 內部維護的已選項目清單
  selectedOptions = signal<SelectOption[]>([]);

  // 🌟 將 FormControl 的值優雅轉換為 Signal，取代 RxJS 的 valueChanges
  searchValue = toSignal(this.searchControl.valueChanges, { initialValue: '' });

  // 🌟 使用 computed 處理過濾邏輯，當 options 或 searchValue 改變時自動重算，極致效能！
  filteredOptions = computed(() => {
    const search = (this.searchValue() || '').toLowerCase();
    return this.options().filter(option => option.label?.toLowerCase().includes(search)).slice(0, 100);
  });

  // ControlValueAccessor callbacks
  private onChange: (value: any) => void = () => { };
  private onTouched: () => void = () => { };

  // --- CVA 介面實作 ---
  writeValue(value: any): void {
    if (!value) {
      this.selectedOptions.set([]);
      return;
    }

    if (this.multiple() && Array.isArray(value)) {
      this.selectedOptions.set(this.options().filter(opt => value.includes(opt.value)));
    } else {
      const selected = this.options().find(opt => opt.value === value);
      this.selectedOptions.set(selected ? [selected] : []);
    }
  }

  registerOnChange(fn: any): void { this.onChange = fn; }
  registerOnTouched(fn: any): void { this.onTouched = fn; }

  // --- UI 互動邏輯 ---
  togglePanel() {
    this.isOpen.update(v => !v); // 🌟 修正：正確的 Signal toggle 寫法
    if (this.isOpen()) {
      setTimeout(() => this.searchInput()?.nativeElement.focus());
    } else {
      this.searchControl.setValue('');
      this.onTouched();
    }
  }

  closePanel() {
    this.isOpen.set(false);
    this.searchControl.setValue('');
    this.onTouched();
  }

  toggleOption(option: SelectOption) {
    if (this.multiple()) {
      // 🌟 修正：使用 update 且不直接 mutate 陣列 (Immutability 原則)
      this.selectedOptions.update(opts => {
        const index = opts.findIndex(opt => opt.value === option.value);
        if (index > -1) {
          return opts.filter(opt => opt.value !== option.value); // 移除
        } else {
          return [...opts, option]; // 新增
        }
      });
    } else {
      this.selectedOptions.set([option]);
      this.closePanel();
    }
    this.emitValue();
    this.change.emit(option);
  }

  removeOption(option: SelectOption, event: Event) {
    event.stopPropagation();
    // 🌟 修正：使用 update 回傳新陣列
    this.selectedOptions.update(opts => opts.filter(opt => opt.value !== option.value));
    this.emitValue();
  }

  isSelected(option: SelectOption): boolean {
    return this.selectedOptions().some(opt => opt.value === option.value);
  }

  private emitValue() {
    const value = this.multiple()
      ? this.selectedOptions().map(opt => opt.value)
      : (this.selectedOptions()[0]?.value || null);
    this.onChange(value);
  }
}
