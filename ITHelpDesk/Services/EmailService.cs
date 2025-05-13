using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using ITHelpDesk.Domain;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task SendEmailAsync(List<string> recipients, string subject, string body)
        {
            try
            {
                if (recipients == null || !recipients.Any()) return;

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_configuration["MailSettings:SenderName"], _configuration["MailSettings:SenderEmail"]));

                foreach (var recipient in recipients)
                {
                    email.To.Add(new MailboxAddress(recipient, recipient));
                }

                email.Subject = subject;
                email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_configuration["MailSettings:SmtpServer"], int.Parse(_configuration["MailSettings:Port"]), false);
                await smtp.AuthenticateAsync(_configuration["MailSettings:SenderEmail"], _configuration["MailSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"✅ Email sent to: {string.Join(", ", recipients)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
            }
        }


        public async Task SendNewUserNotificationToAdmins(ApplicationUser user)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var itUsers = await _userManager.GetUsersInRoleAsync("IT");

            var recipients = admins.Concat(itUsers).Select(u => u.Email).ToList();
            if (!recipients.Any()) return;

            var subject = "New User Registration";
            var message = $"A new user ({user.Email}) has registered and is waiting for confirmation.";

            await SendEmailAsync(recipients, subject, message);
        }

        public async Task SendAccountUnderReviewNotification(string email)
        {
            var subject = "Account Under Review";
            var message = "Your registration is successful, but your account is under review. You will be notified once it is approved.";

            await SendEmailAsync(new List<string> { email }, subject, message);
        }

        public async Task SendAccountApprovalNotification(string email)
        {
            var subject = "Account Approved";
            var message = "Your account has been approved. You can now log in.";

            await SendEmailAsync(new List<string> { email }, subject, message);
        }

        public async Task SendUserConfirmedNotificationToAdmins(ApplicationUser user)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var itUsers = await _userManager.GetUsersInRoleAsync("IT");

            var recipients = admins.Concat(itUsers).Select(u => u.Email).ToList();
            if (!recipients.Any()) return;

            var subject = "User Confirmed";
            var message = $"User ({user.Email}) has been approved by Admin/IT.";

            await SendEmailAsync(recipients, subject, message);
        }

    }
}
