// src/app/core/interceptors/api-prefix.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '@environments/environment';

export const apiPrefixInterceptor: HttpInterceptorFn = (req, next) => {
  // 如果是完整的外部網址 (例如打第三方 API)，就不做處理
  const isAbsoluteUrl = req.url.startsWith('http');

  if (!isAbsoluteUrl) {
    // 複製原本的 Request，並將 URL 替換為加上 Prefix 的版本
    const apiReq = req.clone({
      url: `${environment.apiUrl}/${req.url}`
    });
    return next(apiReq);
  }

  return next(req);
};