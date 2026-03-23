using BuildingMaintainerWebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuildingMaintainerWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SheetsService _sheetsService;

        public List<string> SheetNames { get; set; } = [];

        public IndexModel(SheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        public async Task OnGetAsync()
        {
            SheetNames = await _sheetsService.GetSheetNamesAsync();
        }
    }
}
