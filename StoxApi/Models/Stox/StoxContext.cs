using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StoxApi.Models.Stox;

public partial class StoxContext : DbContext
{
    public StoxContext()
    {
    }

    public StoxContext(DbContextOptions<StoxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CapitalFlow> CapitalFlow { get; set; }

    public virtual DbSet<InventoryLot> InventoryLot { get; set; }

    public virtual DbSet<StockDailyPrice> StockDailyPrice { get; set; }

    public virtual DbSet<StockMaster> StockMaster { get; set; }

    public virtual DbSet<Transaction> Transaction { get; set; }

    public virtual DbSet<TransactionDetail> TransactionDetail { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:StoxDB");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CapitalFlow>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_CapitalFlow_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF__CapitalFl__Creat__08B54D69");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("TWD", "DF__CapitalFl__Curre__07C12930");
            entity.Property(e => e.FlowType).HasComment("0-兩者 1-投入本金 2-提領本金");
            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.CapitalFlow)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CapitalFlow_User");
        });

        modelBuilder.Entity<InventoryLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC073017AB67");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OriginalQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RemainingQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StockCode).HasMaxLength(20);
            entity.Property(e => e.StockName).HasMaxLength(100);

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.InventoryLotCreatedUser)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryLot_User_Created");

            entity.HasOne(d => d.Transaction).WithMany(p => p.InventoryLot)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryLot_Transaction");

            entity.HasOne(d => d.UpdatedUser).WithMany(p => p.InventoryLotUpdatedUser)
                .HasForeignKey(d => d.UpdatedUserId)
                .HasConstraintName("FK_InventoryLot_User_Updated");
        });

        modelBuilder.Entity<StockDailyPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockDai__3214EC07491B0C75");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdjClose).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ClosePrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.HighPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.LowPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.OpenPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StockCode).HasMaxLength(20);
        });

        modelBuilder.Entity<StockMaster>(entity =>
        {
            entity.ToTable(tb => tb.HasComment(""));

            entity.HasIndex(e => new { e.Symbol, e.Name, e.IsActive }, "IX_StockMaster_Search");

            entity.HasIndex(e => e.SearchKeywords, "IX_StockMaster_SearchKeywords");

            entity.HasIndex(e => new { e.Symbol, e.Market }, "IX_StockMaster_Symbol_Market").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasComment("計價幣別 (例: 'USD', 'TWD'，計算投資組合總值時必備)");
            entity.Property(e => e.IsActive)
                .HasComment("狀態 (1=正常交易, 0=已下市。下市不刪除資料，以免歷史交易紀錄關聯報錯)")
                .HasDefaultValue(true, "DF__StockMast__IsAct__70DDC3D8");
            entity.Property(e => e.Market)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComment("市場別 (例: 'US', 'TW'，用來區分台股或美股)");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasComment("股票名稱");
            entity.Property(e => e.SearchKeywords)
                .HasMaxLength(300)
                .HasComment("隱藏的搜尋關鍵字 (例: '台積電,tsmc,護國神山,2330')");
            entity.Property(e => e.Sector)
                .HasMaxLength(50)
                .HasComment("產業別 (例: 'Technology', 'Semiconductors'，方便做產業分佈圓餅圖)");
            entity.Property(e => e.SortOrder).HasComment("權重排序 (熱門股如 NVDA, TSLA, 0050 可以設高一點，下拉選單會排前面)");
            entity.Property(e => e.Symbol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("股票代號");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasComment("資產類型 (例: 'Stock' 個股, 'ETF' 指數型基金)");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07C3EF2666");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Fee).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StockCode).HasMaxLength(20);
            entity.Property(e => e.StockName).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.Transaction)
                .HasForeignKey(d => d.CreatedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_User");
        });

        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0731DCC10B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RealizedProfit).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.InventoryLot).WithMany(p => p.TransactionDetail)
                .HasForeignKey(d => d.InventoryLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionDetail_InventoryLot");

            entity.HasOne(d => d.SellTransaction).WithMany(p => p.TransactionDetail)
                .HasForeignKey(d => d.SellTransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionDetail_SellTransaction");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07411AF776");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())", "DF__User__Id__4AB81AF0");
            entity.Property(e => e.Account).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())", "DF__User__CreatedDat__4BAC3F29");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(90);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
