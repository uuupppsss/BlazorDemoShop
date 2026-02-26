using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class ProductImageDTO
    {
        public int Id { get; set; }
        public string Image { get; set; } = null!;
        public int ProductId { get; set; }
    }

    public class CreateProductImageDTO
    {
        public string Image { get; set; } = null!;
        public int ProductId { get; set; }
    }
}
