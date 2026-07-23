namespace ProductManagement.Domain.Entities
{
    /// <summary>
    /// Item entity representing a specific quantity tracking of a Product.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique identifier for the item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the parent product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Quantity of the item. Must be greater than zero (validated by FluentValidation).
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Parent product entity navigation property.
        /// </summary>
        public virtual Product? Product { get; set; }
    }
}
