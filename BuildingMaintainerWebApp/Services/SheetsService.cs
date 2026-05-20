using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildingMaintainerWebApp.Services
{
    public class SheetsService
    {
        private class ServiceAccountDetails
        {
            [JsonPropertyName("client_email")]
            public string? ClientEmail { get; set; }

            [JsonPropertyName("private_key")]
            public string? PrivateKey { get; set; }
        }

        private readonly MaintainerConfig _config;
        private readonly Google.Apis.Sheets.v4.SheetsService _sheetsService;

        public SheetsService(IOptions<MaintainerConfig> config)
        {
            _config = config.Value;
            _sheetsService = GetSheetsService();
        }

        private Google.Apis.Sheets.v4.SheetsService GetSheetsService()
        {
            var json = File.ReadAllText(_config.CredentialsPath);
            var details = JsonSerializer.Deserialize<ServiceAccountDetails>(json);

            if (
                details is null
                || string.IsNullOrEmpty(details.ClientEmail)
                || string.IsNullOrEmpty(details.PrivateKey)
            )
            {
                throw new InvalidOperationException("Invalid credential file.");
            }

            var initializer = new ServiceAccountCredential.Initializer(details.ClientEmail)
            {
                Scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.SpreadsheetsReadonly },
            }.FromPrivateKey(details.PrivateKey);

            var credential = new ServiceAccountCredential(initializer);

            return new Google.Apis.Sheets.v4.SheetsService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Building Maintainer App",
                }
            );
        }

        public async Task<List<string>> GetSheetNamesAsync()
        {
            var sheetNames = new List<string>();
            var request = _sheetsService.Spreadsheets.Get(_config.SpreadsheetId);
            var response = await request.ExecuteAsync();
            if (response != null)
            {
                foreach (var sheet in response.Sheets)
                {
                    sheetNames.Add(sheet.Properties.Title);
                }
            }
            return sheetNames;
        }

        public async Task<List<Dictionary<string, string>>> GetSheetDataAsync(string sheetName)
        {
            var data = new List<Dictionary<string, string>>();
            var request = _sheetsService.Spreadsheets.Values.Get(
                _config.SpreadsheetId,
                $"{sheetName}!A:Z"
            );
            var response = await request.ExecuteAsync();
            if (response.Values != null && response.Values.Count > 1)
            {
                var headers = response.Values[0].Select(h => h.ToString()).ToList();
                for (int i = 1; i < response.Values.Count; i++)
                {
                    var row = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Count && j < response.Values[i].Count; j++)
                    {
                        row[headers[j]] = response.Values[i][j].ToString();
                    }
                    data.Add(row);
                }
            }
            return data;
        }
    }
}
