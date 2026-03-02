using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace ApiDemoShop.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessageAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email не может быть пустым.", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Код подтверждения не может быть пустым.", nameof(code));
            }

            var host = _configuration["Smtp:Host"];
            var senderEmail = _configuration["Smtp:SenderEmail"];
            var senderPassword = _configuration["Smtp:SenderPassword"];
            var senderName = _configuration["Smtp:SenderName"] ?? "DemoShop";
            var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
            var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var parsedEnableSsl) ? parsedEnableSsl : true;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new InvalidOperationException("SMTP настройки не заданы. Проверьте секцию Smtp в appsettings.");
            }

            using var message = new MailMessage(
                new MailAddress(senderEmail, senderName),
                new MailAddress(email))
            {
                Subject = "Код подтверждения email",
                Body = $"Для подтверждения регистрации используйте код: {code}",
                IsBodyHtml = false
            };

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };

            _logger.LogInformation("Отправка кода подтверждения регистрации на {Email}", email);
            await smtp.SendMailAsync(message, cancellationToken);
        }

        public string CreateCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString(CultureInfo.InvariantCulture);
        }
    }
}
