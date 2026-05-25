namespace ApiDemoShop.Model
{
    public class Promotion
    {
        public int Id { get; set; }

        /// Скидка в процентах
        public double Discount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
