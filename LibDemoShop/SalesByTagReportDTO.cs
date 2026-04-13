namespace LibDemoShop
{
    public class SalesByTagReportDTO
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalSoldItems { get; set; }
        public List<SalesByTagSoldProductDTO> TopProducts { get; set; } = new();
        public List<SalesByTagReportItemDTO> Items { get; set; } = new();
    }

    public class SalesByTagReportItemDTO
    {
        public int TagId { get; set; }
        public string TagTitle { get; set; } = string.Empty;
        public string? TagTypeTitle { get; set; }
        public int SoldItems { get; set; }
        public int DistinctProducts { get; set; }
    }

    public class SalesByTagProductsReportDTO
    {
        public int TagId { get; set; }
        public string TagTitle { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalSoldItems { get; set; }
        public List<SalesByTagSoldProductDTO> Items { get; set; } = new();
    }

    public class SalesByTagSoldProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SoldItems { get; set; }
        public decimal Revenue { get; set; }
        public int ProductStockCount { get; set; }
    }
}
