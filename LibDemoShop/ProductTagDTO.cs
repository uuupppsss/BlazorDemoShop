using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class ProductTagDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
        public string? TagTitle { get; set; }
    }

    public class CreateProductTagDTO
    {
        public int ProductId { get; set; }
        public int TagId { get; set; }
    }
}
