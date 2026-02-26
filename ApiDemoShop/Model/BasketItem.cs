namespace ApiDemoShop.Model
{
    public class BasketItem
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
