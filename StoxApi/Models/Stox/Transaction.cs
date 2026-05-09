using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class Transaction
{
    public Guid Id { get; set; }

    public string StockCode { get; set; } = null!;

    public string? StockName { get; set; }

    public DateTime TradeDate { get; set; }

    public int ActionType { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Fee { get; set; }

    public decimal TotalAmount { get; set; }

    public Guid CreatedUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User CreatedUser { get; set; } = null!;

    public virtual ICollection<InventoryLot> InventoryLot { get; set; } = new List<InventoryLot>();

    public virtual ICollection<TransactionDetail> TransactionDetail { get; set; } = new List<TransactionDetail>();
}
