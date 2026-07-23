namespace ProductManagement.Domain.Entities;

public class Item
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public virtual Product? Product { get; set; }
}
