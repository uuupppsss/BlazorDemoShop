namespace ApiDemoShop.Model
{
    public class Product
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TimeBought { get; set; }

        public virtual ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
        public virtual ICollection<SavedProduct> SavedProducts { get; set; } = new List<SavedProduct>();
    }
}
