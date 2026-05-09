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

    public virtual DbSet<InventoryLot> InventoryLot { get; set; }

    public virtual DbSet<StockDailyPrice> StockDailyPrice { get; set; }

    public virtual DbSet<Transaction> Transaction { get; set; }

    public virtual DbSet<TransactionDetail> TransactionDetail { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:StoxDB");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
