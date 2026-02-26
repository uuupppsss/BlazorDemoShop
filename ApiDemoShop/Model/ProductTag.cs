namespace ApiDemoShop.Model
{
    public class ProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
