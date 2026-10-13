namespace BookRest.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public Instant CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }
}
