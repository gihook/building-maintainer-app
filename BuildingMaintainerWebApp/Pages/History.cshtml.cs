using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingMaintainerWebApp.Models;
using BuildingMaintainerWebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuildingMaintainerWebApp.Pages
{
    public class HistoryModel : PageModel
    {
        private readonly SheetsService _sheetsService;

        public List<ReplacementHistory> History { get; set; }

        public HistoryModel(SheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        public async Task OnGetAsync()
        {
            History = await _sheetsService.GetReplacementHistoryAsync();
        }
    }
}
