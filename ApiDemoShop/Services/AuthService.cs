using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace ApiDemoShop.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> SignUpAsync(CreateUserDTO request);
        Task<AuthResponseDTO> SignInAsync(LoginDTO request);
        Task<AuthResponseDTO> ConfirmEmailAsync(ConfirmEmailDTO request);
        Task<AuthResponseDTO> LogoutAsync();
        Task<UserInfoDTO?> GetUserByIdAsync(int id);
    }

    public class AuthService : IAuthService
    {
        private readonly DemoShopDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly int _emailCodeLifetimeMinutes;

        public AuthService(
            DemoShopDbContext context,
            JwtService jwtService,
            EmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _emailCodeLifetimeMinutes = int.TryParse(configuration["AuthConfirmation:CodeLifetimeMinutes"], out int lifetime)
                ? lifetime : 10;
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

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = HashService.HashMethod(request.Password),
                    ContactPhone = request.ContactPhone,
                    RoleId = 2,
                    IsEmailConfirmed = false
                };

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await CreateOrRefreshVerificationCodeAsync(user);

                await transaction.CommitAsync();

                return new AuthResponseDTO
                {
                    Success = true,
                    RequiresEmailConfirmation = true,
                    Message = "Регистрация успешна. Код подтверждения отправлен на email",
                    UserName = user.Username,
                    Email = user.Email,
                    UserId = user.Id
                };
            }
            catch (SmtpException)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Не удалось отправить код подтверждения на почту"
                };
            }
            catch (InvalidOperationException ex)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch
            {
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

                var isPasswordValid = user.Password == HashService.HashMethod(request.Password);
                if (!isPasswordValid)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Неверный email или пароль"
                    };
                }

                if (!user.IsEmailConfirmed)
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    await CreateOrRefreshVerificationCodeAsync(user);
                    await transaction.CommitAsync();

                    return new AuthResponseDTO
                    {
                        Success = false,
                        RequiresEmailConfirmation = true,
                        Message = "Email не подтвержден. Новый код отправлен на почту",
                        UserName = user.Username,
                        Email = user.Email,
                        UserId = user.Id
                    };
                }

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
            catch (SmtpException)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Не удалось отправить код подтверждения на почту"
                };
            }
            catch (InvalidOperationException ex)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Внутренняя ошибка сервера"
                };
            }
        }

        public async Task<AuthResponseDTO> ConfirmEmailAsync(ConfirmEmailDTO request)
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
                        Message = "Пользователь не найден"
                    };
                }

                if (user.IsEmailConfirmed)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Email уже подтвержден. Выполните вход"
                    };
                }

                var verificationCode = await _context.EmailVerificationCodes
                    .FirstOrDefaultAsync(v => v.UserId == user.Id);

                if (verificationCode == null)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Код подтверждения не найден. Запросите код заново"
                    };
                }

                if (verificationCode.ExpiresAt < DateTime.UtcNow)
                {
                    _context.EmailVerificationCodes.Remove(verificationCode);
                    await _context.SaveChangesAsync();

                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Срок действия кода истек. Выполните вход, чтобы получить новый код"
                    };
                }

                var requestCodeHash = HashService.HashMethod(request.Code);
                if (verificationCode.CodeHash != requestCodeHash)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Неверный код подтверждения"
                    };
                }

                user.IsEmailConfirmed = true;
                _context.EmailVerificationCodes.Remove(verificationCode);
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(user);
                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Email подтвержден",
                    Token = token,
                    UserName = user.Username,
                    Email = user.Email,
                    UserId = user.Id,
                    TokenExpiration = DateTime.UtcNow.AddDays(7)
                };
            }
            catch
            {
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
            {
                return null;
            }

            return new UserInfoDTO
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email
            };
        }

        public Task<AuthResponseDTO> LogoutAsync()
        {
            return Task.FromResult(new AuthResponseDTO
            {
                Success = true,
                Message = "Выход выполнен успешно"
            });
        }

        private async Task CreateOrRefreshVerificationCodeAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("Для пользователя не задан email");
            }

            var code = _emailService.CreateCode();
            var now = DateTime.UtcNow;
            var codeHash = HashService.HashMethod(code);

            var verificationCode = await _context.EmailVerificationCodes
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (verificationCode == null)
            {
                verificationCode = new EmailVerificationCode
                {
                    UserId = user.Id,
                    CodeHash = codeHash,
                    CreatedAt = now,
                    ExpiresAt = now.AddMinutes(_emailCodeLifetimeMinutes)
                };

                _context.EmailVerificationCodes.Add(verificationCode);
            }
            else
            {
                verificationCode.CodeHash = codeHash;
                verificationCode.CreatedAt = now;
                verificationCode.ExpiresAt = now.AddMinutes(_emailCodeLifetimeMinutes);
            }

            await _context.SaveChangesAsync();
            await _emailService.SendMessageAsync(user.Email, code);
        }
    }
}
