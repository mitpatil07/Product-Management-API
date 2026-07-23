using System.Collections.Generic;
using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}

