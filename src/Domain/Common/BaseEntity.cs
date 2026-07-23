using System;

namespace ProductManagement.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

