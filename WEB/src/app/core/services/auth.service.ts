import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5200/api/auth'; // 替換為你的 .NET 10 API 網址

  login(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.apiUrl}/login`, { username, password }).pipe(
      tap((response) => {
        // 登入成功後，將 Token 存入 localStorage
        localStorage.setItem('jwt_token', response.token);
      }),
    );
  }

  logout() {
    // 登出就是將 Token 刪除
    localStorage.removeItem('jwt_token');
  }

  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  // 檢查 Token 是否過期
  isTokenExpired(token: string): boolean {
    if (!token) return true;

    try {
      // JWT 格式為 header.payload.signature，取中間的 payload
      const payloadBase64 = token.split('.')[1];
      // 解析 base64 (注意：實務上可能需要處理 base64url 的替換)
      const decodedJson = atob(payloadBase64);
      const payload = JSON.parse(decodedJson);

      // JWT 的 exp 單位是秒，Date.now() 是毫秒，所以要乘 1000
      const expirationDate = payload.exp * 1000;

      // 如果現在時間大於過期時間，代表已過期
      return Date.now() > expirationDate;
    } catch (error) {
      // 解析失敗就當作無效/過期
      return true;
    }
  }
}
