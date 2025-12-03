using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class EPostaService
    {
        private readonly string _fromEposta = "kamirbai07@gmail.com"; // gönderen adres
        private readonly string _password = "vmzd gjbd edmk nfzt";
        private readonly UrlSigningService _urlSigningService;

        public EPostaService(UrlSigningService urlSigningService)
        {
            _urlSigningService = urlSigningService;
        }
        public async Task SendResetEpostaAsync(string toEposta, string token, string kullaniciAdi)
        {
            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEposta, _password),
                EnableSsl = true
            };

            string subject = "Şifre Sıfırlama Linkiniz";
            
            // URL'i imzala
            var linkParams = new Dictionary<string, string> { { "token", token } };
            string link = _urlSigningService.SignUrl("https://localhost:7014/sifre-sifirla", linkParams);
            
            string body =
                $"Merhaba {kullaniciAdi},\n\n" +
                $"Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:\n{link}\n\n" +
                $"Bu bağlantı 3 dakika boyunca geçerlidir.\n\n" +
                $"İyi günler dileriz.";

            using var message = new MailMessage(_fromEposta, toEposta, subject, body);
            await smtp.SendMailAsync(message);
        }

        public async Task SendYeniGorevBildirimiAsync(string toEposta, string kullaniciAdi, string gorevBaslik, string dashboardUrl)
        {
            try
            {
                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_fromEposta, _password),
                    EnableSsl = true
                };

                string subject = "Yeni Göreviniz Var!";
                string body =
                    $"Merhaba {kullaniciAdi},\n\n" +
                    $"Size yeni bir görev atandı:\n" +
                    $"Görev: {gorevBaslik}\n\n" +
                    $"Görevi görüntülemek için aşağıdaki bağlantıya tıklayın:\n{dashboardUrl}\n\n" +
                    $"İyi çalışmalar dileriz.";

                using var message = new MailMessage(_fromEposta, toEposta, subject, body);
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"E-posta gönderme hatası: {ex.Message}");
                // Hata olsa bile devam et, e-posta gönderimi kritik değil
            }
        }
    }
}
