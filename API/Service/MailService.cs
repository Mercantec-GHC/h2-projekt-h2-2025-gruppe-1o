using DomainModels.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Services
{
    /// <summary>
    /// En service til at håndtere afsendelse af e-mails via SendGrid API'et.
    /// </summary>
    public class MailService
    {
        private readonly SendGridSettings _sendGridSettings;
        private readonly ILogger<MailService> _logger;

        public MailService(IOptions<SendGridSettings> sendGridSettings, ILogger<MailService> logger)
        {
            _sendGridSettings = sendGridSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(_sendGridSettings.ApiKey);
                var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", body);

                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("E-mail sendt succesfuldt til {ToEmail} via SendGrid.", toEmail);
                    return true;
                }

                _logger.LogError("SendGrid afviste at sende e-mail til {ToEmail}. Statuskode: {StatusCode}. Fejl: {ErrorBody}",
                    toEmail, response.StatusCode, await response.Body.ReadAsStringAsync());
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uventet fejl under afsendelse af e-mail via SendGrid til {ToEmail}.", toEmail);
                return false;
            }
        }
    }
}