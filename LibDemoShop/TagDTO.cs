using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class TagDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int TypeId { get; set; }
        public string? TypeTitle { get; set; }
    }

    public class CreateTagDTO
    {
        public string Title { get; set; } = null!;
        public int TypeId { get; set; }
    }

    public class UpdateTagDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int? TypeId { get; set; }
    }
}
