using System.Net;
using System.Net.Mail;

namespace ApiDemoShop.Services
{
    public class EmailService
    {
        //private SmtpClient smtp;

        private static EmailService instance;
        public static EmailService Instance {  
            get 
            {
                if(instance == null)
                {
                    instance = new EmailService();
                    //instance.smtp=new SmtpClient("smtp.beget.com", 2525);
                    //instance.smtp.Credentials = new NetworkCredential("1145@suz-ppk.ru","i_love_PPK!1");
                    //instance.smtp.EnableSsl = true;
                }
                return instance;
            } 
        }

        public async Task SendMessageasync(string email)
        {
            MailAddress from= new MailAddress("1145@suz-ppk.ru");
            MailAddress to = new MailAddress(email);

            string code = CreateCode();
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Код авторизации";
            message.Body = $"Для подтверждения входа используйте код {code}";
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.beget.com", 2525);
            smtp.Credentials = new NetworkCredential("1145@suz-ppk.ru", "i_love_PPK!1");
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(message);
        }

        public string CreateCode()
        {
            return "superswcretcode";
        }
    }
}
