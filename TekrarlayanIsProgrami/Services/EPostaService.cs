using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class EPostaService
    {
        private readonly string _fromE_posta = "sengingmail@gmail.com";
        private readonly string _password = "uygulama_sifresi";

        public async Task SendResetE_postaAsync(string toE_posta, string token)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromE_posta, _password),
                EnableSsl = true
            };

            string subject = "Şifre Sıfırlama Linkiniz";
            string body = $"Şifre sıfırlama linkiniz: https://example.com/reset-password?token={token}\n" +
                          "Link 30 dakika geçerlidir.";

            await smtp.SendMailAsync(_fromE_posta, toE_posta, subject, body);
        }
    }
}
