using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.App.Services
{
    public class MailSmtpService : IMailService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _settings;
        private readonly int _emailIntervalMinutes;
        private DateTime LastEmailSent;
        public MailSmtpService(ILogger<MailSmtpService> logger, IConfiguration settings)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            _settings = settings ?? throw new ArgumentNullException("IConfiguration");
            _emailIntervalMinutes = _settings.GetValue<int>("EmailIntervalMinutes", 10);
        }

        public Task<CommandResult> SendMail(string to, string subject, string body)
        {
            try
            {
                if (LastEmailSent != default
                    && DateTime.UtcNow < LastEmailSent + TimeSpan.FromMinutes(_emailIntervalMinutes))
                {
                    _logger.LogInformation("Email interval minutes reached, waiting...");
                    return Task.FromResult(new CommandResult(true, "Email interval minutes reached"));
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = _settings.GetValue<string>("MailSettings:PrimaryDomain");
                    smtpClient.EnableSsl = _settings.GetValue<bool>("MailSettings:PrimarySsl");
                    smtpClient.Port = _settings.GetValue<int>("MailSettings:PrimaryPort");
                    smtpClient.Credentials = new NetworkCredential(_settings.GetValue<string>("MailSettings:UsernameEmail"), _settings.GetValue<string>("MailSettings:UsernamePassword"));

                    MailMessage message = new MailMessage();
                    MailAddress fromAddress, toAddress, ccAddress, bccAddress;
                    var fromEmail = _settings.GetValue<string>("MailSettings:FromEmail");
                    _logger.LogDebug($"From e-mail: {fromEmail}");
                    var fromDisplayName = _settings.GetValue<string>("MailSettings:FromDisplayName");
                    _logger.LogDebug($"From display name: {fromDisplayName}");
                    var ccEmail = _settings.GetValue<string>("MailSettings:CcEmail");
                    _logger.LogDebug($"From ccEmail : {ccEmail}");
                    var bccEmail = _settings.GetValue<string>("MailSettings:BccEmail");
                    _logger.LogDebug($"From bccEmail : {bccEmail}");

                    fromAddress = !string.IsNullOrWhiteSpace(fromDisplayName) ? new MailAddress(fromEmail, fromDisplayName) : new MailAddress(fromEmail);
                    toAddress = !string.IsNullOrWhiteSpace(to) ? new MailAddress(to) : null;
                    ccAddress = !string.IsNullOrWhiteSpace(ccEmail) ? new MailAddress(ccEmail) : null;
                    bccAddress = !string.IsNullOrWhiteSpace(bccEmail) ? new MailAddress(bccEmail) : null;

                    if (fromAddress != null) message.From = fromAddress;
                    if (toAddress != null) message.To.Add(toAddress);
                    if (ccAddress != null) message.CC.Add(ccAddress);
                    if (bccAddress != null) message.Bcc.Add(bccAddress);

                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    _logger.LogInformation($"Sending e-mail: subject {message.Subject}");
                    _logger.LogTrace($"Sending e-mail: subject {message.Subject} \n Body: {message.Body}" +
                        $" \n SMTP UsernameEmail: {_settings.GetValue<string>("MailSettings:UsernameEmail")} " +
                        $" \n SMTP UsernamePassword: { _settings.GetValue<string>("MailSettings:UsernamePassword")} ");

                    smtpClient.Send(message);
                }
                LastEmailSent = DateTime.UtcNow;
                return Task.FromResult(new CommandResult(true, "Success"));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An exception ocurred while trying to send mail");
                return Task.FromResult(new CommandResult(false, "Exception", null, HttpStatusCode.InternalServerError));
            }
        }
    }
}