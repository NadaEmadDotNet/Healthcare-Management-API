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
        public async Task<AuthResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // اعمل ديكود للتوكن قبل التأكيد
            var decodedToken = System.Web.HttpUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                Success = true,
                Message = "Email confirmed successfully"
            };
        }


    }
}
