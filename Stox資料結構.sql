USE Stox
GO
DROP TABLE [TransactionDetails]
DROP TABLE [InventoryLots]
DROP TABLE [Transactions]
DROP TABLE [Users]
GO
-- 使用者
create table [Users](
	[Id]            UNIQUEIDENTIFIER PRIMARY KEY,
	[Account]       VARCHAR(20) NOT NULL,
	[Password]      NVARCHAR(500) NOT NULL,
	[Email]         VARCHAR(60) NULL,
	[CreatedDate]   DATETIME NOT NULL,
	[UpdatedDate]   DATETIME NULL,
);

-- 1. 交易流水帳：記錄所有動作的原始事實
CREATE TABLE [Transactions] (
    [Id]            UNIQUEIDENTIFIER PRIMARY KEY,
    [StockCode]     NVARCHAR(20) NOT NULL,
    [StockName]     NVARCHAR(50),
    [TradeDate]     DATETIME NOT NULL,
    [ActionType]    INT NOT NULL, -- 1-BUY, 2-SELL, 3-STOCK_DIV (配股)
    [Quantity]      DECIMAL(18, 4) NOT NULL, -- 考慮美股碎股用 decimal
    [Price]         DECIMAL(18, 4) NOT NULL, -- 成交單價
    [Fee]           DECIMAL(18, 2) NOT NULL, -- 手續費/稅金
    [TotalAmount]   DECIMAL(18, 2) NOT NULL, -- 最終結算金額
    [CreatedUserId] UNIQUEIDENTIFIER NOT NULL,
	[CreatedDate] DATETIME NOT NULL,
);

-- 2. 庫存批次：記錄「現在倉庫裡還有什麼」
CREATE TABLE [InventoryLots] (
    [Id]            UNIQUEIDENTIFIER PRIMARY KEY,
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,            -- 關聯回原始買入的那筆交易
    [StockCode]     NVARCHAR(20) NOT NULL,
    [StockName]     NVARCHAR(50),
    [BuyDate]       DATETIME NOT NULL,       -- 用於 FIFO 排序
    [OriginalQty]   DECIMAL(18, 4) NOT NULL, -- 原始買入數量
    [RemainingQty]  DECIMAL(18, 4) NOT NULL, -- 剩餘可賣數量 (賣完變 0，不刪除)
    [CostPrice]     DECIMAL(18, 4) NOT NULL, -- 含手續費後的單位成本
    [CreatedUserId] UNIQUEIDENTIFIER NOT NULL,
	[CreatedDate]   DATETIME NOT NULL,
	[UpdatedUserId] UNIQUEIDENTIFIER NULL,
	[UpdatedDate]   DATETIME NULL,

    FOREIGN KEY (TransactionId) REFERENCES [Transactions](Id),
);

-- 3. 交易沖銷明細：連結「賣出交易」與「被扣除的庫存」
CREATE TABLE [TransactionDetails] (
    [Id]               UNIQUEIDENTIFIER PRIMARY KEY,
    [SellTransactionId]UNIQUEIDENTIFIER NOT NULL,           -- 指向那筆 'SELL' 交易
    [InventoryLotId]   UNIQUEIDENTIFIER NOT NULL,           -- 指向被扣掉的那個庫存批次
    [Qty]              DECIMAL(18, 4) NOT NULL, -- 從該批次扣了多少股
    [RealizedProfit]   DECIMAL(18, 2) NOT NULL, -- 這部分的實現損益

    FOREIGN KEY (SellTransactionId) REFERENCES [Transactions](Id),
    FOREIGN KEY (InventoryLotId) REFERENCES [InventoryLots](Id)
);