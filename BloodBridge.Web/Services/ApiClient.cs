using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BloodBridge.Web.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        SendAsync<T>(() => _httpClient.GetAsync(path, cancellationToken));

    public Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default) =>
        SendAsync<T>(() => _httpClient.PostAsJsonAsync(path, body, cancellationToken));

    public Task<T> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default) =>
        SendAsync<T>(() => _httpClient.PutAsJsonAsync(path, body, cancellationToken));

    public async Task PutNoContentAsync(string path, object body, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(path, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(path, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private static async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await send();
        await EnsureSuccessAsync(response);
        var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: default);
        return value ?? throw new ApiException((int)response.StatusCode, "The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var text = await response.Content.ReadAsStringAsync();
        var message = text;
        try
        {
            using var json = JsonDocument.Parse(text);
            if (json.RootElement.TryGetProperty("message", out var messageProperty))
            {
                message = messageProperty.GetString() ?? text;
            }
        }
        catch (JsonException)
        {
            // Keep the raw response when the API did not return JSON.
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            message = "Your session has expired. Please log in again.";
        }

        throw new ApiException((int)response.StatusCode, message);
    }
}

public sealed class ApiException : Exception
{
    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
