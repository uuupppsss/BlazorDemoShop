using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDemoShop.Model
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RecieveDate { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
        public string? Address { get; set; } = null;
        public int DeliveryMethodId { get; set; }
        public decimal DeliveryPrice { get; set; }
        public string? TrackingLink { get; set; }

        public virtual OrderStatus Status { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        //[ForeignKey(nameof(DeliveryMethodId))]
        public virtual DeliveryMethod DeliveryMethod { get; set; } = null!;


        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
