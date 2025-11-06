using System.Net;
using System.Net.Mail;

namespace TekrarlayanIsProgrami.Services
{
    public class SifreService
    {
        public static async Task<bool> SifreSifirlamaMailiGonder(string email, string token)
        {
            string link = $"https://localhost:7014/sifre-sifirla?token={token}";
            Console.WriteLine($"[TEST] {email} adresi için sıfırlama bağlantısı: {link}");
            await Task.CompletedTask;
            return true;
            //try
            //{
            //    var mail = new MailMessage();
            //    mail.From = new MailAddress("ankapromosyon@gmail.com");
            //    mail.To.Add(email);
            //    mail.Subject = "Şifre Sıfırlama Bağlantısı";
            //    mail.Body = $"Şifreni sıfırlamak için tıkla: https://localhost:7014/sifre-sifirla?token={token}";
            //    mail.IsBodyHtml = true;

            //    var smtp = new SmtpClient("smtp.gmail.com", 587);
            //    smtp.Credentials = new NetworkCredential("ejejehdhdj78@gmail.com", "uygulama_sifresi"); //Gerçek projede burda gerçek google hesabımız olcak ve şifre kendi şifremiz değil farklı bir şifre alcaz.
            //    smtp.EnableSsl = true;

            //    await smtp.SendMailAsync(mail);
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Mail gönderim hatası: " + ex.Message);
            //    return false;
            //}
        }
    }
}
