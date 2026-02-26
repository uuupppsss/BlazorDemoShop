using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class SavedProductDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public string? ProductImage { get; set; }
    }

    public class CreateSavedProductDTO
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
    }
}
