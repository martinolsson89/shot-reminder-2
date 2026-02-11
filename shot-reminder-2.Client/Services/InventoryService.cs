using shot_reminder_2.Contracts.Inventory;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace shot_reminder_2.Client.Services;

public sealed class InventoryService(HttpClient http, AuthService auth)
{
    public async Task AddStockAsync(AddStockRequest request, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/inventory/add")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Add stock failed", ct);
        }
    }

    public async Task RestockAsync(AddStockRequest request, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/inventory/restock")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Restock failed", ct);
        }
    }

    public async Task UpdateInventoryAsync(AddStockRequest request, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Put, "api/inventory/update")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Update inventory failed", ct);
        }
    }

    public async Task DeleteInventoryAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Delete, "api/inventory");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Delete inventory failed", ct);
        }
    }

    public async Task<GetInventoryResponse?> GetInventoryAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/inventory");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);
            else if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            throw await CreateApiExceptionAsync(response, "Get inventory failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<GetInventoryResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    private static async Task<Exception> CreateApiExceptionAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new InvalidOperationException("Your session has expired. Please log in again.");

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            var details = await TryReadApiMessageAsync(response, ct);

            return new InvalidOperationException(string.IsNullOrWhiteSpace(details)
                ? "Please verify your input and try again."
                : details);
        }

        return new InvalidOperationException($"{operation} ({(int)response.StatusCode}).");
    }

    private static async Task<string?> TryReadApiMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        string? raw;
        try
        {
            raw = await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            var error = JsonSerializer.Deserialize<ApiError>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(error?.Message))
                return error.Message;
        }
        catch
        {
            // If not JSON payload, keep raw content as fallback.
        }

        return raw;
    }

    private sealed record ApiError(string? Message);
}
