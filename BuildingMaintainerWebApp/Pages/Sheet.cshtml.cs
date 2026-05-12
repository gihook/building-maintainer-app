using BuildingMaintainerWebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuildingMaintainerWebApp.Pages
{
    public class SheetModel : PageModel
    {
        private readonly SheetsService _sheetsService;

        public List<Dictionary<string, string>> SheetData { get; set; } = [];
        public List<string> Headers { get; set; } = [];
        public string SheetName { get; set; } = string.Empty;

        public SheetModel(SheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        public async Task OnGetAsync(string sheetName)
        {
            SheetName = sheetName.Replace("-", " ");
            SheetData = await _sheetsService.GetSheetDataAsync(SheetName);
            if (SheetData.Any())
            {
                Headers = SheetData.First().Keys.ToList();
            }
            else
            {
                Headers = new List<string>();
            }
        }
    }
}
