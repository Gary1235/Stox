// auth.model.ts
export interface JwtPayload {
  Account?: string;  // 對應 new Claim("Account", user.Account)
  UserName?: string;
  Role?: string;     // 對應 new Claim("Role", "Admin")
  UserId?: string;   // 對應 new Claim("UserId", user.Id.ToString())
  exp: number;       // JWT 內建的過期時間
}

// 供 UI 使用的乾淨模型 (維持不變)
export interface UserProfile {
  userId: string;
  accountName: string;
  userName: string;
  role: string;
  avatarUrl: string;
  isAdmin: boolean; 
}