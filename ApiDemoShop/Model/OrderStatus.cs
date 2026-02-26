namespace ApiDemoShop.Model
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
