namespace ApiDemoShop.Model
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal FullCost { get; set; }
        public DateTime? RecieveDate { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }

        public virtual OrderStatus Status { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
