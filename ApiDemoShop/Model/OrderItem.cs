namespace ApiDemoShop.Model
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int OrdeId { get; set; }
        public int ProductId { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
