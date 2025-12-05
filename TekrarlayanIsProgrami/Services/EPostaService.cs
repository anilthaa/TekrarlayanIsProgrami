using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class EPostaService
    {
        private readonly string _fromEposta = "kamirb1820@gmail.com";
        private readonly string _password = "mrxy gsgo iphm rcfx";    /*vmzd gjbd edmk nfzt*/
        private readonly UrlSigningService _urlSigningService;

        public EPostaService(UrlSigningService urlSigningService)
        {
            _urlSigningService = urlSigningService;
        }

        private SmtpClient CreateClient()
        {
            return new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_fromEposta, _password),
                EnableSsl = true
            };
        }

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            using var smtp = CreateClient();
            using var msg = new MailMessage(_fromEposta, to, subject, body)
            {
                IsBodyHtml = isHtml
            };
            await smtp.SendMailAsync(msg);
        }

        public async Task SendResetEpostaAsync(string toEposta, string token, string kullaniciAdi)
        {
            var linkParams = new Dictionary<string, string> { { "token", token } };
            string link = _urlSigningService.SignUrl("https://localhost:7014/sifre-sifirla", linkParams);

            string body =
                $"Merhaba {kullaniciAdi},<br><br>" +
                $"Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:<br>" +
                $"<a href='{link}'>Şifre Sıfırlama Linki</a><br><br>" +
                $"Bu bağlantı 3 dakika geçerlidir.";

            await SendEmailAsync(toEposta, "Şifre Sıfırlama Linkiniz", body);
        }

        public async Task SendYeniGorevBildirimiAsync(string toEposta, string kullaniciAdi, string gorevBaslik, string dashboardUrl)
        {
            string body =
                $"Merhaba {kullaniciAdi},<br><br>" +
                $"Yeni bir göreviniz var:<br>" +
                $"<b>{gorevBaslik}</b><br><br>" +
                $"Görevi görmek için: <a href='{dashboardUrl}'>Tıklayın</a>";

            await SendEmailAsync(toEposta, "Yeni Göreviniz Var", body);
        }
        public async Task SendInviteMailAsync(string email, string evetLink, string hayirLink)
        {
            var subject = "Hesap Onay Daveti";
            var body = $@"
        <p>Merhaba,</p>
        <p>Hesabınız oluşturuldu. Devam etmek için seçim yapın:</p>
        <p>
            <a href='{evetLink}'>Evet, hesabı aktifleştir</a><br/>
            <a href='{hayirLink}'>Hayır, reddediyorum</a>
        </p>
    ";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordMailAsync(string email, string tempPassword)
        {
            string body =
                $"<p>Hesabınız onaylandı. Geçici şifreniz:</p>" +
                $"<h2>{tempPassword}</h2>" +
                $"<p>Giriş yaptıktan sonra şifreyi değiştirmeniz gerekir.</p>";

            await SendEmailAsync(email, "Geçici Şifreniz", body);
        }

    }
}
