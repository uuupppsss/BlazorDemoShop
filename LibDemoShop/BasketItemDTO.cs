using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class BasketItemDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int ProductAvailableCount { get; set; }

        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public string? ProductImage { get; set; }
    }

    public class CreateBasketItemDTO
    {
        public int Count { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
    }

    public class UpdateBasketItemDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }
}
