using BuildingMaintainerWebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuildingMaintainerWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SheetsService _sheetsService;
        private readonly WahaService _wahaService;

        public List<string> SheetNames { get; set; } = [];
        public bool IsWahaConnected { get; private set; }

        public IndexModel(SheetsService sheetsService, WahaService wahaService)
        {
            _sheetsService = sheetsService;
            _wahaService = wahaService;
        }

        public async Task OnGetAsync()
        {
            SheetNames = await _sheetsService.GetSheetNamesAsync();
            IsWahaConnected = await _wahaService.AreCredentialsValid();
        }
    }
}
