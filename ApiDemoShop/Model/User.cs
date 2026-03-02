namespace ApiDemoShop.Model
{
    public class User
    {
        public int Id { get; set; }
        public string? ContactPhone { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public int RoleId { get; set; } = 2;
        public string Username { get; set; } = null!;
        public bool IsEmailConfirmed { get; set; }

        public virtual UserRole Role { get; set; } = null!;
        public virtual ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
        public virtual ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = new List<EmailVerificationCode>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<SavedProduct> SavedProducts { get; set; } = new List<SavedProduct>();
    }
}
