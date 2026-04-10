import { Component, HostListener, OnDestroy, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class ModalComponent implements OnDestroy {
  // 控制 Modal 開關狀態 (預設關閉)
  isOpen = input<boolean>(false);

  // 尺寸設定 (預設中型)
  size = input<'small' | 'medium' | 'large' | 'fullscreen'>('medium');

  // 是否允許點擊背景關閉 (預設允許)
  closeOnBackdropClick = input<boolean>(true);

  // 是否顯示預設的 X 關閉按鈕 (預設顯示)
  showCloseBtn = input<boolean>(true);

  // 關閉事件發射器 (通知父層把 isOpen 改成 false)
  closeModal = output<void>();

  constructor() {
    // 💡 專家技巧：使用 effect 動態監聽 Signal 的變化
    effect(() => {
      if (this.isOpen()) {
        // Modal 打開時，鎖定背景滾動
        document.body.style.overflow = 'hidden';
      } else {
        // Modal 關閉時，恢復背景滾動
        document.body.style.overflow = '';
      }
    });
  }

  ngOnDestroy(): void {
    // 預防萬一：如果整個元件被 Angular 強制銷毀，確保背景滾動有被恢復
    document.body.style.overflow = '';
  }

  // 監聽 ESC 鍵
  @HostListener('document:keydown.escape')
  onKeydownHandler() {
    // 確保只有在 Modal 打開時，按 ESC 才有作用
    if (this.isOpen()) {
      this.close();
    }
  }

  // 點擊背景遮罩
  onBackdropClick(event: MouseEvent): void {
    if (this.closeOnBackdropClick() && event.target === event.currentTarget) {
      this.close();
    }
  }

  // 觸發關閉事件
  close(): void {
    this.closeModal.emit();
  }
}
