import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { JwtPayload, UserProfile } from '@models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5200/api/auth'; // 替換為你的 .NET 10 API 網址

  // 💎 核心狀態：使用 Signal 管理 Token
  // 初始化時會自動去 localStorage 抓取上次登入的 Token
  private readonly token = signal<string | null>(localStorage.getItem('jwt_token'));

  // 💎 衍生狀態 (User Profile)：只要 token 改變，這裡就會自動重新計算，且具備快取效能
  public readonly currentUser = computed<UserProfile | null>(() => {
    const currentToken = this.token();
    
    if (!currentToken || this.isTokenExpired(currentToken)) {
      return null; 
    }

    const payload = this.decodeToken(currentToken);
    if (!payload) return null;

    // 從乾淨的 Claim 中取出資料，提供給 UI 渲染
    const accountName = payload.Account || 'Unknown';
    const role = payload.Role || 'User';

    return {
      userId: payload.UserId || '',
      accountName,
      userName: payload.UserName || '',
      role,
      isAdmin: role === 'Admin',
      // 動態產生高質感漸層頭像
      avatarUrl: `https://api.dicebear.com/7.x/initials/svg?seed=${accountName}&backgroundColor=0f172a,1e293b`
    };
  });

  // 💎 衍生狀態 (Auth Status)：取代原本的 isLoggedIn() 方法
  // 現在只要在 Template 寫 @if (authService.isLoggedIn()) 就能響應式切換畫面！
  public readonly isLoggedIn = computed<boolean>(() => !!this.currentUser());

  // ==========================================
  // HTTP 與 登入/登出 操作
  // ==========================================

  login(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.apiUrl}/login`, { username, password }).pipe(
      tap((response) => {
        // 登入成功後：1. 存入實體空間  2. 更新 Signal 觸發全域畫面渲染
        localStorage.setItem('jwt_token', response.token);
        this.token.set(response.token);
      })
    );
  }

  logout() {
    // 登出：1. 清除實體空間  2. 清空 Signal 狀態
    localStorage.removeItem('jwt_token');
    this.token.set(null);
  }

  getToken(): string | null {
    // 直接回傳 Signal 的當前值
    return this.token();
  }

  // ==========================================
  // JWT 解析與驗證工具
  // ==========================================

  // 檢查 Token 是否過期 (保留你的核心邏輯，加上更安全的解碼)
  isTokenExpired(token: string): boolean {
    if (!token) return true;

    try {
      const payload = this.decodeToken(token);
      if (!payload || !payload.exp) return true;

      // JWT 的 exp 單位是秒，Date.now() 是毫秒
      const expirationDate = payload.exp * 1000;
      return Date.now() > expirationDate;
    } catch (error) {
      return true; // 發生任何錯誤都視為無效/過期
    }
  }

  // 強化版的 JWT 解析器，處理了 Base64Url 的符號替換，避免 Decode 崩潰
  private decodeToken(token: string): JwtPayload | null {
    try {
      const payloadBase64 = token.split('.')[1];
      // 替換 URL-safe 字符，這是實務上極重要的細節！
      const base64 = payloadBase64.replace(/-/g, '+').replace(/_/g, '/');
      
      const decodedJson = decodeURIComponent(
        atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
      );
      
      return JSON.parse(decodedJson) as JwtPayload;
    } catch (error) {
      console.error('JWT 解析失敗', error);
      return null;
    }
  }
}
