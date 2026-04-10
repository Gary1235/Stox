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

    public virtual DbSet<InventoryLot> InventoryLots { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:StoxDB");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC075B881B13");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BuyDate).HasColumnType("datetime");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.OriginalQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RemainingQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StockCode).HasMaxLength(20);
            entity.Property(e => e.StockName).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Transaction).WithMany(p => p.InventoryLots)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Trans__08B54D69");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07FA520E67");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Fee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.StockCode).HasMaxLength(20);
            entity.Property(e => e.StockName).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TradeDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07998EBD2A");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RealizedProfit).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.InventoryLot).WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.InventoryLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Inven__0C85DE4D");

            entity.HasOne(d => d.SellTransaction).WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.SellTransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__SellT__0B91BA14");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07FB05B842");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Account)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
