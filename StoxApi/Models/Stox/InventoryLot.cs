using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class InventoryLot
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public string StockCode { get; set; } = null!;

    public string? StockName { get; set; }

    public DateTime BuyDate { get; set; }

    public decimal OriginalQty { get; set; }

    public decimal RemainingQty { get; set; }

    public decimal CostPrice { get; set; }

    public Guid CreatedUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual User CreatedUser { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual ICollection<TransactionDetail> TransactionDetail { get; set; } = new List<TransactionDetail>();

    public virtual User? UpdatedUser { get; set; }
}
