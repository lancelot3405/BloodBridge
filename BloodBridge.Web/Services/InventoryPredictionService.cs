using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BloodBridge.Web.ViewModels;

namespace BloodBridge.Web.Services;

public sealed class InventoryPredictionService
{
    private static readonly string[] BloodGroups =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public InventoryPredictionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // Fetch all eight forecasts and compare them with simple virtual stock values.
    public async Task<InventoryDashboardViewModel> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var virtualInventory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["A+"] = 20,
            ["A-"] = 8,
            ["B+"] = 18,
            ["B-"] = 6,
            ["AB+"] = 10,
            ["AB-"] = 4,
            ["O+"] = 24,
            ["O-"] = 8
        };

        var forecasts = new List<InventoryForecastViewModel>();
        try
        {
            foreach (var bloodGroup in BloodGroups)
            {
                var groupForecast = await GetForecastAsync(
                    bloodGroup,
                    virtualInventory[bloodGroup],
                    cancellationToken);
                forecasts.AddRange(groupForecast);
            }
        }
        catch (HttpRequestException)
        {
            return new InventoryDashboardViewModel
            {
                Error = "The inventory prediction service is not running. Start FastAPI to view forecasts.",
                VirtualInventoryNote = "Inventory values are virtual demo values until a stock table is added."
            };
        }
        catch (JsonException)
        {
            return new InventoryDashboardViewModel
            {
                Error = "The inventory prediction service returned an invalid forecast response.",
                VirtualInventoryNote = "Inventory values are virtual demo values until a stock table is added."
            };
        }

        return new InventoryDashboardViewModel
        {
            Forecasts = forecasts,
            VirtualInventoryNote = "Inventory values are virtual demo values until a stock table is added."
        };
    }

    // Call the FastAPI router for one blood group and add the shortage-risk flag.
    private async Task<List<InventoryForecastViewModel>> GetForecastAsync(
        string bloodGroup,
        int virtualInventory,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["InventoryPrediction:BaseUrl"] ?? "http://localhost:8000/";
        var client = _httpClientFactory.CreateClient("InventoryPrediction");
        var forecast = await client.GetFromJsonAsync<List<InventoryForecastResponse>>(
            $"{baseUrl.TrimEnd('/')}/forecast-demand/{bloodGroup}",
            cancellationToken) ?? [];

        return forecast.Select(item => new InventoryForecastViewModel
        {
            Date = item.Date,
            BloodGroup = item.BloodGroup,
            PredictedDemand = item.PredictedDemand,
            VirtualInventory = virtualInventory,
            HighShortageRisk = item.PredictedDemand > virtualInventory * 1.10
        }).ToList();
    }
}

// This DTO matches one forecast item returned by FastAPI.
public sealed class InventoryForecastResponse
{
    [JsonPropertyName("date")]
    public DateTime Date { get; init; }

    [JsonPropertyName("blood_group")]
    public string BloodGroup { get; init; } = string.Empty;

    [JsonPropertyName("predicted_demand")]
    public double PredictedDemand { get; init; }
}
