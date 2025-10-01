using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class EPostaService
    {
        private readonly string _fromEmail = "sengingmail@gmail.com";
        private readonly string _password = "uygulama_sifresi";

        public async Task SendResetEmailAsync(string toEmail, string token)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEmail, _password),
                EnableSsl = true
            };

            string subject = "Şifre Sıfırlama Linkiniz";
            string body = $"Şifre sıfırlama linkiniz: https://example.com/reset-password?token={token}\n" +
                          "Link 30 dakika geçerlidir.";

            await smtp.SendMailAsync(_fromEmail, toEmail, subject, body);
        }
    }
}
