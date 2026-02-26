using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class OrderStatusDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
    }

    public class CreateOrderStatusDTO
    {
        public string Title { get; set; } = null!;
    }
}
