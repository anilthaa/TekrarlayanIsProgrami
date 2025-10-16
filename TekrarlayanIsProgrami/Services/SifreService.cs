using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class SifreService
    {
        public static async Task<bool> SifreSifirlamaMailiGonder(string email, string token)
        {
            try
            {
                var mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("noreply@seninsite.com");
                mail.Subject = "Şifre Sıfırlama Bağlantısı";
                mail.Body = $"Şifrenizi sıfırlamak için tıklayın:\nhttps://seninsite.com/sifre-sifirla?token={token}";

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new System.Net.NetworkCredential("noreply@seninsite.com", "mail_sifresi"),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
