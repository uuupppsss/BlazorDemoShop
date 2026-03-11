using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TimeBought { get; set; }

        public List<string> Images { get; set; } = new();
        public List<TagDTO> Tags { get; set; } = new();
        public string? MainImage { get; set; }
    }

    public class CreateProductDTO
    {
        public int Count { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TimeBought { get; set; }
        public List<int>? TagIds { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class UpdateProductDTO
    {
        public int Id { get; set; }
        public int? Count { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? TimeBought { get; set; }
        public List<int>? TagIds { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
