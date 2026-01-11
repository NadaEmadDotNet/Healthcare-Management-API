using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Http;
using System.Net.Mail;

namespace Medication_Reminder_API.Application.Services
{
    public class EmailService: IEmailService 
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmailService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
 
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:Host"],
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                EnableSsl = true,

                Credentials= new NetworkCredential(
                    _configuration["EmailSettings:Email"],
                    _configuration["EmailSettings:Password"]
                    )
            };
            var message = new MailMessage(
           _configuration["EmailSettings:Email"],
           to,
           subject,
           body
       );
            await smtpClient.SendMailAsync(message);

        }

    }
}
