export enum CapitalFlowType {
  Both = 0,
  Deposit = 1,
  Withdraw = 2,
}

export interface CapitalFlowSearchViewModel {
  startDate: string; // ISO Date String
  endDate: string; // ISO Date String
  flowType: CapitalFlowType;
}

export interface CapitalFlowViewModel {
  id?: string; // 新增時不需要 id
  userId: string;
  userName?: string;
  flowType: CapitalFlowType;
  amount: number;
  currency?: string;
  transactionDate: string;
  note?: string;
}

export interface CapitalSummaryViewModel {
  totalTwdCapital: number;
  lastTransactionDate: string;
}

export interface ApiResponse<T = any> {
  message: string;
  id?: string;
}