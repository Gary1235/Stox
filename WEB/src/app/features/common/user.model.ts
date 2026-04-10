// 基底介面：只放「安全」且「絕對共用」的欄位
export interface UserBaseDto {
  account: string;
  email: string | null;
}

// 新增 (Request)：需要密碼，不需要 ID
export class CreateUserDto implements UserBaseDto {
  account: string = '';
  email: string | null = null;
  password: string = ''; // 密碼只在註冊/新增時出現
}

// 修改 (Request)：需要 ID，通常不包含密碼 (密碼應由獨立 API 處理)
export class UpdateUserDto implements UserBaseDto {
  id: string = '';
  account: string = '';
  email: string | null = null;
  // 如果你的情境允許同時修改密碼，可以加上 password?: string; (設為選填)
}

// 列表/查詢 (Response)：需要 ID，絕對不可有密碼！
export class ListUserDto implements UserBaseDto {
  id: string = '';
  account: string = '';
  email: string | null = null;
}
