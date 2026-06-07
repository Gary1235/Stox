/// <summary>
/// 投入本金總覽
/// </summary>
public class CapitalSummaryViewModel
{
    public decimal TotalTwdCapital { get; set; }

    public DateTime LastTransactionDate { get; set; }
}

public class CapitalFlowSearchViewModel
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);

    public DateTime EndDate { get; set; } = DateTime.UtcNow;

    public CapitalFlowType FlowType { get; set; } = CapitalFlowType.Both;
}

public class CapitalFlowViewModel
{
    public Guid Id { get; set; } 

    public Guid UserId { get; set;}

    public string? UserName { get; set;}

    public CapitalFlowType FlowType { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set;}

    public DateTime TransactionDate { get; set; }

    public string? Note { get; set; }
}