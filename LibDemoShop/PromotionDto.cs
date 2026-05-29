using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class CreateUpdatePromotionDto
    {
        public double Discount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int ProductId { get; set; }
    }

    public class PromotionDto
    {
        public int Id { get; set; }

        public double Discount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
    }

    public class PromoInfo
    {
        public bool HasDiscount { get; set; } = false;
        public double? OldPrice { get; set; }
        public double? FinalPrice { get; set; }
        public double? DiscountPercent { get; set; }
        public DateTime? EndDate { get; set; } = null;
    }
}
