namespace ApiDemoShop.Model
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CodeHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
