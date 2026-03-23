using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BuildingMaintainerWebApp.Services;

public class WahaChat
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WahaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WahaService> _logger;

    public WahaService(HttpClient httpClient, IConfiguration configuration, ILogger<WahaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var baseUrl = configuration["WAHA_API_URL"] ?? "http://localhost:3000";
        _httpClient.BaseAddress = new Uri(baseUrl);

        var apiKey = configuration["WAHA_API_KEY"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }
    }

    public async Task<bool> AreCredentialsValid()
    {
        try
        {
            // WAHA normally returns a 401/403 if the API key is missing or incorrect
            // Calling a simple read endpoint like /api/sessions is a good way to verify this.
            var response = await _httpClient.GetAsync("/api/sessions");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify WAHA credentials.");
            return false;
        }
    }

    public async Task<List<WahaChat>> GetChatsAsync(string session = "default")
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/{session}/chats");
            response.EnsureSuccessStatusCode();
            var chats = await response.Content.ReadFromJsonAsync<List<WahaChat>>();
            return chats ?? new List<WahaChat>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chats from WAHA.");
            return new List<WahaChat>();
        }
    }

    public async Task<bool> SendMessageAsync(string chatId, string text, string session = "default")
    {
        try
        {
            var payload = new
            {
                session = session,
                chatId = chatId,
                text = text
            };
            var response = await _httpClient.PostAsJsonAsync("/api/sendText", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message via WAHA.");
            return false;
        }
    }
}
