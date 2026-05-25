namespace LibDemoShop
{
    public class ProductCardDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string MainImage { get; set; } = string.Empty;

        // ¿ ÷»ﬂ

        public bool HasDiscount { get; set; } = false;
        public double? OldPrice { get; set; }
        public double? FinalPrice { get; set; }
        public double? DiscountPercent { get; set; }
    }
}
