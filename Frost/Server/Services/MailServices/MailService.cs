using System.Net;
using System.Net.Mail;

namespace Frost.Server.Services.MailServices
{
    public interface IMailService
    {
        public void NotifyUserAboutNewOfferAsync(string userMail, string offerUrl);
        public void NotifyUserAboutExpiringOfferAsync(string userMail, string offerTitle);
        public void NotifyUserAboutChangedLoginDetailsAsync(string userMail);
        public void NotifyUserAboutNewMessageAsync(string userMail, string userName);
    }
    public class MailService : IMailService
    {
        private IConfiguration _configuration;
        private string _serviceMail;
        private string _password;
        public MailService(IConfiguration configuration) {
            _configuration = configuration;
            _serviceMail = _configuration["Mail:Login"];
            _password = _configuration["Mail:Password"];
        }
        public async void NotifyUserAboutNewOfferAsync(string userMail,string offerUrl)
        {

            string content = $"Pojawiła się nowa oferta, która może cię interesować: \n {offerUrl}";
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587){EnableSsl = true,Credentials = new NetworkCredential(_serviceMail, _password) })
            {
                await client.SendMailAsync(new MailMessage(from: _serviceMail, to: userMail, subject: "Nowa oferta!", content));
            }
        }
        public async void NotifyUserAboutExpiringOfferAsync(string userMail,string offerTitle)
        {
            string content = $"Twoja oferta:{offerTitle} niedługo wygaśnie, zapisz ją w szablonach aby móc udostępnić ją ponownie po jej wygaśnięciu";
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, Credentials = new NetworkCredential(_serviceMail, _password) })
            {
                await client.SendMailAsync(new MailMessage(from: _serviceMail, to: userMail, subject: "Twoja oferta wygasa!", content));
            }
        }
        public async void NotifyUserAboutChangedLoginDetailsAsync(string userMail)
        {
            string content = $"Szczegóły konta: {userMail} zostały niedawno zmienione";
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, Credentials = new NetworkCredential(_serviceMail, _password) })
            {
                await client.SendMailAsync(new MailMessage(from: _serviceMail, to: userMail, subject: "Zmiana szczegółów konta", content));
            }
        }
        public async void NotifyUserAboutNewMessageAsync(string userMail, string userName)
        {
            string content = $"Użytkownik {userName} wysłał ci wiadomość!";
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, Credentials = new NetworkCredential(_serviceMail, _password) })
            {
                await client.SendMailAsync(new MailMessage(from: _serviceMail, to: userMail, subject: "Nowa wiadomość", content));
            }
        }
    }
}
