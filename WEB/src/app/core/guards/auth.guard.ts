// auth.guard.ts
import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '@services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const token = authService.getToken();

  // 同時檢查「是否有 Token」以及「Token 是否還在有效期限內」
  if (token && !authService.isTokenExpired(token)) {
    return true;
  } else {
    // 可能是沒 Token，或者是過期了，清除舊 Token 並導回 login
    localStorage.removeItem('token'); // 記得清掉無效的 Token
    router.navigate(['/login']);
    return false;
  }
};
