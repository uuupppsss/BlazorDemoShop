using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class ProductTypeDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public List<TagDTO>? Tags { get; set; }
    }

    public class CreateProductTypeDTO
    {
        public string Title { get; set; } = null!;
    }
}
