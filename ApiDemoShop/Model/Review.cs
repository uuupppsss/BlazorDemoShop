namespace ApiDemoShop.Model
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Product Product { get; set; } = null!;
        public virtual User User { get; set; }=null!;
    }
}
