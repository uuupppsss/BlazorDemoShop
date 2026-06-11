using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? RecieveDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusTitle { get; set; }
        public int UserId { get; set; }
        public decimal DeliveryPrice { get; set; }
        public string? TrackingLink { get; set; }
        public int DeliveryMethodId { get; set; }
        public string? Address { get; set; }

        public decimal FullCost { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string DeliveryName { get; set; } = null!;


        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }

    public class CreateOrderDTO
    {
        public int DeliveryMethodId { get; set; }
        public string? Address { get; set; }

        //public List<CreateOrderItemDTO> OrderItems { get; set; } = new();
    }

    public class UpdateOrderStatusDTO
    {
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public DateTime? RecieveDate { get; set; }
        public string? TrackingLink { get; set; }
    }


}
