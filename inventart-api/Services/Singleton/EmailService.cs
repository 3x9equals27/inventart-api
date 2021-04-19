using Inventart.Config;
using Inventart.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net.Mail;
using System.Net;

namespace Inventart.Services.Singleton
{
    public class EmailService
    {
        private readonly SmtpConfig smtpConfig;
        private readonly GlobalConfig globalConfig;
        public EmailService(IOptions<SmtpConfig> smtpConfig, IOptions<GlobalConfig> globalConfig)
        {
            this.smtpConfig = smtpConfig.Value;
            this.globalConfig = globalConfig.Value;
        }
        public void SendVerificationLink(string registrationEmail, Guid verificationGuid)
        {
            string fromAddress = smtpConfig.Address;
            var toAddress = new MailAddress(registrationEmail);
            string fromPassword = smtpConfig.Password;
            const string subject = "Inventart verification link";
            string body = $"Please verify your email using the link bellow:<BR/> {globalConfig.VerificationPrefix}{verificationGuid}";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromAddress, "Inventart");
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(smtpConfig.Host, smtpConfig.Port))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}
