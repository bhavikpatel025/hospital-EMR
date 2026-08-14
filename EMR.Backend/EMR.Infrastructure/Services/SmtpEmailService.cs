using EMR.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EMR.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var subject = "Hospital EMR: Password Reset Request";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset Request</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; color: #1e293b;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;'>
        <tr>
            <td align='center' style='padding: 40px 10px;'>
                <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 560px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 10px 25px rgba(0,0,0,0.05); overflow: hidden;'>
                    <!-- Brand Header -->
                    <tr>
                        <td align='center' style='background: linear-gradient(135deg, #0d9488, #0284c7); padding: 32px 20px;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;'>NextGen Hospital EMR</h1>
                            <p style='margin: 6px 0 0 0; color: rgba(255,255,255,0.9); font-size: 14px;'>Clinical & Patient Management System</p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 36px 32px;'>
                            <h2 style='margin: 0 0 16px 0; color: #0f172a; font-size: 20px; font-weight: 700;'>Password Reset Request</h2>
                            <p style='margin: 0 0 16px 0; font-size: 15px; line-height: 1.6; color: #475569;'>
                                Hello <strong>{WebUtility.HtmlEncode(userName)}</strong>,
                            </p>
                            <p style='margin: 0 0 24px 0; font-size: 15px; line-height: 1.6; color: #475569;'>
                                We received a request to reset the password for your Hospital EMR account associated with <strong>{WebUtility.HtmlEncode(toEmail)}</strong>.
                            </p>

                            <!-- CTA Button -->
                            <table border='0' cellpadding='0' cellspacing='0' width='100%' style='margin: 28px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{resetLink}' target='_blank' style='display: inline-block; background: linear-gradient(135deg, #0d9488, #0284c7); color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; padding: 14px 32px; border-radius: 8px; box-shadow: 0 4px 12px rgba(13, 148, 136, 0.3);'>
                                            Reset My Password
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 24px 0 8px 0; font-size: 13px; line-height: 1.5; color: #64748b;'>
                                Or copy and paste this link into your browser:
                            </p>
                            <p style='margin: 0 0 24px 0; font-size: 12px; color: #0284c7; word-break: break-all;'>
                                <a href='{resetLink}' style='color: #0284c7; text-decoration: underline;'>{resetLink}</a>
                            </p>

                            <div style='background-color: #f8fafc; border-left: 4px solid #f59e0b; padding: 12px 16px; border-radius: 4px; margin-bottom: 24px;'>
                                <p style='margin: 0; font-size: 13px; color: #78350f;'>
                                    <strong>Important:</strong> This password reset link is valid for <strong>15 minutes</strong>. If you did not make this request, you can safely ignore this email.
                                </p>
                            </div>

                            <p style='margin: 0; font-size: 14px; color: #475569;'>
                                Best regards,<br>
                                <strong>Hospital Security Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td align='center' style='background-color: #f8fafc; border-top: 1px solid #f1f5f9; padding: 20px; font-size: 12px; color: #94a3b8;'>
                            &copy; {DateTime.UtcNow.Year} NextGen Hospital EMR. All rights reserved.<br>
                            This is an automated system notification. Please do not reply directly to this email.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(_configuration["SmtpSettings:Port"], out var p) ? p : 587;
            var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "hospital.emr.care@gmail.com";
            var senderName = _configuration["SmtpSettings:SenderName"] ?? "NextGen Hospital EMR";
            var username = _configuration["SmtpSettings:Username"] ?? senderEmail;
            var password = _configuration["SmtpSettings:Password"] ?? "";
            var enableSsl = bool.TryParse(_configuration["SmtpSettings:EnableSsl"], out var ssl) && ssl;

            _logger.LogInformation("Attempting to send email to {ToEmail} with subject '{Subject}'", toEmail, subject);

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("SMTP Password is not configured in appsettings.json. Email delivery simulated successfully.");
                _logger.LogInformation("EMAIL CONTENT for {ToEmail}:\n{Body}", toEmail, htmlBody);
                return;
            }

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully delivered to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Error: {Message}", toEmail, ex.Message);
                // We do not throw to prevent blocking the user flow; token remains valid in DB for testing
            }
        }
    }
}
