namespace BlazorDemoShop.Services
{
    public class SessionSignInRequest
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
