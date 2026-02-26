using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.EntityFrameworkCore;

namespace ApiDemoShop.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> SignUpAsync(CreateUserDTO request);
        Task<AuthResponseDTO> SignInAsync(LoginDTO request);
        Task<UserInfoDTO> GetUserByIdAsync(int id);
    }

    public class AuthService : IAuthService
    {
        private readonly DemoShopDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(DemoShopDbContext context, JwtService jwtService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDTO> SignUpAsync(CreateUserDTO request)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

                if (existingUser != null)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Пользователь с таким email или именем уже существует"
                    };
                }

                string passwordHash = HashService.HashMethod(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = passwordHash,
                    ContactPhone = request.ContactPhone,
                    RoleId = 2
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();


                var token = _jwtService.GenerateToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Регистрация успешна",
                    Token = token,
                    UserName = user.Username,
                    Email = user.Email,
                    UserId = user.Id,
                    TokenExpiration = DateTime.UtcNow.AddDays(7)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                };
            }
        }

        public async Task<AuthResponseDTO> SignInAsync(LoginDTO request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Неверный email или пароль"
                    };
                }

                // Проверяем пароль
                bool isPasswordValid = user.Password == HashService.HashMethod(request.Password);

                if (!isPasswordValid)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Неверный email или пароль"
                    };
                }

                // Генерируем JWT токен
                var token = _jwtService.GenerateToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Вход выполнен успешно",
                    Token = token,
                    UserName = user.Username,
                    Email = user.Email,
                    UserId = user.Id,
                    TokenExpiration = DateTime.UtcNow.AddDays(7)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                };
            }
        }

        public async Task<UserInfoDTO?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            return new UserInfoDTO
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email
            };
        }
    }
}
