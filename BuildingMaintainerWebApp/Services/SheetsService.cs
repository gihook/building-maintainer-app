using BuildingMaintainerWebApp.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildingMaintainerWebApp.Services
{
    public class SheetsService
    {
        private readonly MaintainerConfig _config;
        private readonly Google.Apis.Sheets.v4.SheetsService _sheetsService;

        public SheetsService(IOptions<MaintainerConfig> config)
        {
            _config = config.Value;
            _sheetsService = GetSheetsService();
        }

        private Google.Apis.Sheets.v4.SheetsService GetSheetsService()
        {
            var credential = GoogleCredential
                .FromFile(_config.CredentialsPath)
                .CreateScoped(Google.Apis.Sheets.v4.SheetsService.Scope.SpreadsheetsReadonly);

            return new Google.Apis.Sheets.v4.SheetsService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Building Maintainer App",
                }
            );
        }

        public async Task<List<ReplacementHistory>> GetReplacementHistoryAsync()
        {
            var history = new List<ReplacementHistory>();
            var request = _sheetsService.Spreadsheets.Values.Get(
                _config.SpreadsheetId,
                _config.RangeHistory
            );
            var response = await request.ExecuteAsync();
            if (response.Values != null)
            {
                foreach (var row in response.Values)
                {
                    if (row.Count >= 3)
                    {
                        history.Add(
                            new ReplacementHistory
                            {
                                Stanar = row[0].ToString(),
                                BrojPotrosenihKesa = row[1].ToString(),
                                Datum = row[2].ToString(),
                            }
                        );
                    }
                }
            }
            return history;
        }
    }
}
