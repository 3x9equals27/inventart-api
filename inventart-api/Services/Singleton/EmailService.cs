using Inventart.Config;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;

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
            const string subject = "Inventart verification link";
            string body = $"Please verify your email using the link bellow:<BR/> {globalConfig.VerificationPrefix}{verificationGuid}";

            this.SendMail(registrationEmail, subject, body);
        }
        public void SendPasswordResetLink(string email, Guid guid)
        {
            const string subject = "Inventart password reset";
            string body = $"Password reset link:<BR/> {globalConfig.ResetPasswordStep2}{guid}";

            this.SendMail(email, subject, body);
        }

        private void SendMail(string email, string subject, string body)
        {
            var toAddress = new MailAddress(email);
            string fromAddress = smtpConfig.Address;
            string fromPassword = smtpConfig.Password;
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