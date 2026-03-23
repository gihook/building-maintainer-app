using BuildingMaintainerWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuildingMaintainerWebApp.Pages;

public class WhatsAppModel : PageModel
{
    private readonly WahaService _wahaService;

    public WhatsAppModel(WahaService wahaService)
    {
        _wahaService = wahaService;
    }

    [BindProperty]
    public string SelectedChatId { get; set; } = string.Empty;

    [BindProperty]
    public string MessageText { get; set; } = string.Empty;

    public List<SelectListItem> Chats { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;
    
    [TempData]
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadChatsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedChatId) || string.IsNullOrWhiteSpace(MessageText))
        {
            StatusMessage = "Chat and Message Text are required.";
            IsError = true;
            await LoadChatsAsync();
            return Page();
        }

        var success = await _wahaService.SendMessageAsync(SelectedChatId, MessageText);

        if (success)
        {
            StatusMessage = "Message sent successfully!";
            IsError = false;
            MessageText = string.Empty;
        }
        else
        {
            StatusMessage = "Failed to send message. Check logs.";
            IsError = true;
        }

        await LoadChatsAsync();
        return Page();
    }

    private async Task LoadChatsAsync()
    {
        var chats = await _wahaService.GetChatsAsync();
        
        var namedChats = chats
            .Where(c => !string.IsNullOrEmpty(c.Name))
            .Select(c => new SelectListItem { Value = c.Id, Text = c.Name });
            
        Chats.AddRange(namedChats);
            
        var unnamedChats = chats
            .Where(c => string.IsNullOrEmpty(c.Name))
            .Select(c => new SelectListItem { Value = c.Id, Text = c.Id });
            
        Chats.AddRange(unnamedChats);
    }
}