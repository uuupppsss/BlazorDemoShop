using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoShop
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? ContactPhone { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public string? RoleTitle { get; set; }
        public string Username { get; set; } = null!;
    }

    public class CreateUserDTO
    {
        public string? ContactPhone { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
        public string Username { get; set; } = null!;
    }

    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string? ContactPhone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public string? Username { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ConfirmEmailDTO
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool RequiresEmailConfirmation { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int UserId { get; set; }
        public DateTime TokenExpiration { get; set; }
    }

    public class UserInfoDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
