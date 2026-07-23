namespace ProductManagement.Application.DTOs
{
    /// <summary>
    /// DTO for representing a Product Item.
    /// </summary>
    public class ItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
