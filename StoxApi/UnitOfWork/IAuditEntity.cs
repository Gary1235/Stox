public interface IAuditEntity
{
    /// <summary>
    /// 建立人Id
    /// </summary>
    public Guid CreatedUserId { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedDate { get; set; }
    /// <summary>
    /// 更新人Id
    /// </summary>
    public Guid? UpdatedUserId { get; set; }
    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime? UpdatedDate { get; set; }
}