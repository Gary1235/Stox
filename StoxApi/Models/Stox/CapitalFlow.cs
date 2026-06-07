using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class CapitalFlow
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// 0-兩者 1-投入本金 2-提領本金
    /// </summary>
    public int FlowType { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedUserId { get; set; }

    public DateTime? DeleteAt { get; set; }

    public virtual User User { get; set; } = null!;
}
