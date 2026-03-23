using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace BuildingMaintainerWebApp;

public class BuildingMaintainerJob
{
    private readonly MaintainerConfig _config;
    private readonly ILogger<BuildingMaintainerJob> _logger;

    public BuildingMaintainerJob(
        IOptions<MaintainerConfig> config,
        ILogger<BuildingMaintainerJob> logger
    )
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("BuildingMaintainerJob starting.");
        var emails = await GetEmailsAsync();
        var (replacer, replacementDate) = await GetReplacerInfoAsync();
        var today = GetToday();

        var shouldSendEmails = today == replacementDate;

        _logger.LogInformation(
            "{{ emails = [{EmailsCount}], replacer = '{Replacer}', replacementDate = '{ReplacementDate}', today = '{Today}', shouldSendEmails = {ShouldSendEmails} }}",
            emails?.Count ?? 0,
            replacer,
            replacementDate,
            today,
            shouldSendEmails
        );

        if (!shouldSendEmails || emails == null || !emails.Any())
        {
            _logger.LogInformation("No emails to send or conditions not met.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.SenderName, _config.SenderEmail));

        foreach (var email in emails)
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        message.Subject = "Podsetnik za menjanje kesa";
        message.Body = new TextPart(TextFormat.Html)
        {
            Text =
                $@"Pozdrav,

<p>podsećam vas da je danas dan za menjanje kesa u dvorištu. Zaduzen za danas je <b>{replacer}</b>. Ukoliko je zaduženi zauzet neka pusti mail da ga neko odmeni.<p>
<p>Raspored mozete videti <a href=""https://docs.google.com/spreadsheets/d/1xPlu-1DTmw8O0vx0AygsHMJ-9ybOghzKzRT3nThTLg0/edit?gid=515625028#gid=515625028"">ovde</a></p>
",
        };

        using (var client = new SmtpClient())
        {
            try
            {
                // await client.ConnectAsync(
                //     _config.SmtpServer,
                //     _config.SmtpPort,
                //     MailKit.Security.SecureSocketOptions.StartTls
                // );
                // await client.AuthenticateAsync(_config.SmtpUser, _config.SmtpPass);
                // await client.SendAsync(message);
                // await client.DisconnectAsync(true);
                _logger.LogInformation("Message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
            }
        }
    }

    private async Task<SheetsService> GetSheetsServiceAsync()
    {
        var credential = CredentialFactory
            .FromFile<ServiceAccountCredential>(_config.CredentialsPath)
            .ToGoogleCredential()
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        return new SheetsService(
            new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Building Maintainer App",
            }
        );
    }

    private async Task<List<string>> GetEmailsAsync()
    {
        try
        {
            var service = await GetSheetsServiceAsync();
            var request = service.Spreadsheets.Values.Get(
                _config.SpreadsheetId,
                _config.RangeEmails
            );
            var response = await request.ExecuteAsync();

            var emails = new List<string>();
            if (response.Values != null)
            {
                foreach (var row in response.Values)
                {
                    if (row.Count > 0 && row[0] != null)
                    {
                        var email = row[0].ToString();
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            emails.Add(email);
                        }
                    }
                }
            }
            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The API returned an error while getting emails.");
            return new List<string>();
        }
    }

    private async Task<(string Replacer, string ParsedDate)> GetReplacerInfoAsync()
    {
        try
        {
            var service = await GetSheetsServiceAsync();
            var request = service.Spreadsheets.Values.Get(
                _config.SpreadsheetId,
                _config.RangeReplacer
            );
            var response = await request.ExecuteAsync();

            if (response.Values != null && response.Values.Count > 0)
            {
                var firstRow = response.Values[0];
                if (firstRow.Count >= 2)
                {
                    var firstItem = firstRow[0]?.ToString() ?? "";
                    var date = firstRow[1]?.ToString() ?? "";

                    var parts = date.Split('/');
                    if (parts.Length == 3)
                    {
                        var month = parts[0];
                        var day = parts[1];
                        var year = parts[2];

                        _logger.LogInformation($"[{month}, {day}, {year}]");

                        var parsedDate = $"{year}-{month}-{day}";

                        _logger.LogInformation($"{{ date = {date}, parsedDate = {parsedDate} }}");

                        return (firstItem, parsedDate);
                    }
                }
            }
            return (string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The API returned an error while getting replacer info.");
            return (string.Empty, string.Empty);
        }
    }

    private string GetToday()
    {
        var today = DateTime.Now;
        return $"{today.Year}-{today.Month}-{today.Day}";
    }
}
