using shot_reminder_2.Contracts.Inventory;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace shot_reminder_2.Client.Services;

public sealed class InventoryService(HttpClient http, AuthService auth)
{
    public async Task<GetInventoryResponse> GetInventoryAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/inventory");

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Get latest shot failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<GetInventoryResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    private static async Task<Exception> CreateApiExceptionAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return new InvalidOperationException("Your session has expired. Please log in again.");

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            string? details = null;
            try { details = await response.Content.ReadAsStringAsync(ct); } catch { }

            return new InvalidOperationException(string.IsNullOrWhiteSpace(details)
                ? "Please verify your input and try again."
                : details);
        }

        return new InvalidOperationException($"{operation} ({(int)response.StatusCode}).");
    }
}

