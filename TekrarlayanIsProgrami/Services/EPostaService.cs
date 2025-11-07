using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class EPostaService
    {
        private readonly string _fromEposta = "kamirbai07@gmail.com"; // gönderen adres
        private readonly string _password = "vmzd gjbd edmk nfzt";
        public async Task SendResetEpostaAsync(string toEposta, string token, string kullaniciAdi)
        {
            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEposta, _password),
                EnableSsl = true
            };

            string subject = "Şifre Sıfırlama Linkiniz";
            string link = $"https://localhost:7014/sifre-sifirla?token={token}";
            string body =
                $"Merhaba {kullaniciAdi},\n\n" +
                $"Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:\n{link}\n\n" +
                $"Bu bağlantı 30 dakika boyunca geçerlidir.\n\n" +
                $"İyi günler dileriz.";

            using var message = new MailMessage(_fromEposta, toEposta, subject, body);
            await smtp.SendMailAsync(message);
        }
    }
}
