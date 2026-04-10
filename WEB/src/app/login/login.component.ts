import { Component, signal, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router'; // 建議引入 Router 以便登入成功後跳轉
import { AuthService } from '@services/auth.service'; // 請替換成你實際的 AuthService 路徑

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  // 注入服務
  private authService = inject(AuthService);
  private router = inject(Router);

  // Reactive Form: 建立表單與驗證規則
  form = new FormGroup({
    account: new FormControl<string>('', [Validators.required]),
    password: new FormControl<string>('', [Validators.required, Validators.minLength(6)]),
  });

  // 狀態管理
  isLoading = signal(false);
  showPassword = signal(false);
  loginStatus = signal<'success' | 'error' | null>(null);

  get f() {
    return this.form.controls;
  }

  togglePasswordVisibility() {
    this.showPassword.update((v) => !v);
  }

  // 處理登入送出
  handleLogin(event: Event) {
    event.preventDefault();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.loginStatus.set(null);

    // 取得表單當下的值 (使用 ! 確保型別非空，因為上面已經做過 invalid 檢查)
    const { account, password } = this.form.value;

    // 呼叫 API
    this.authService.login(account!, password!).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.loginStatus.set('success');

        // 登入成功後跳轉到首頁或後台 (可依需求修改路由)
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.loginStatus.set('error');
        console.error('登入失敗:', err);

        // 3秒後移除錯誤提示
        setTimeout(() => this.loginStatus.set(null), 3000);
      },
    });
  }
}
