using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PROTOTIPO.Services
{
    public class EmailServices
    {
        private readonly string _host = "smtp.gmail.com";
        private readonly int _port = 465;

        private readonly string _user =
            "wilfridoantoniorojasmota@gmail.com";

        private readonly string _pass =
            "kewu kpti hmwc wdkf";

        public async Task SendEmailAsync(
            string to,
            string subject,
            string htmlBody)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "Condominio Los Almendros",
                    _user
                )
            );

            message.To.Add(
                MailboxAddress.Parse(to)
            );

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _host,
                _port,
                SecureSocketOptions.SslOnConnect
            );

            await client.AuthenticateAsync(
                _user,
                _pass
            );

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}