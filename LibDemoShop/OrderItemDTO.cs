using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public decimal TotalPrice => (ProductPrice ?? 0) * Count;
    }

    public class CreateOrderItemDTO
    {
        public int Count { get; set; }
        public int ProductId { get; set; }
    }
}
