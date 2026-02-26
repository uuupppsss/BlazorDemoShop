namespace ApiDemoShop.Model
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Image { get; set; } = null!;
        public int ProductId { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
