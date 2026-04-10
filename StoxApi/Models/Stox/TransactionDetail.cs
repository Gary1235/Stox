using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class TransactionDetail
{
    public Guid Id { get; set; }

    public Guid SellTransactionId { get; set; }

    public Guid InventoryLotId { get; set; }

    public decimal Qty { get; set; }

    public decimal RealizedProfit { get; set; }

    public virtual InventoryLot InventoryLot { get; set; } = null!;

    public virtual Transaction SellTransaction { get; set; } = null!;
}
