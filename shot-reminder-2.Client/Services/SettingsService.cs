using shot_reminder_2.Contracts.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace shot_reminder_2.Client.Services;

public sealed class SettingsService(HttpClient http, AuthService auth)
{
    public async Task<GetShotSettingsResponse> GetShotSettingsAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/settings/shots");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Get shot settings failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<GetShotSettingsResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task UpdateShotSettingsAsync(UpdateShotSettingsRequest request, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Put, "api/settings/shots")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Update shot settings failed", ct);
        }
    }

    private static async Task<Exception> CreateApiExceptionAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return new InvalidOperationException("Your session has expired. Please log in again.");

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest
            or System.Net.HttpStatusCode.NotFound
            or System.Net.HttpStatusCode.Conflict)
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
        }

        return raw;
    }

    private sealed record ApiError(string? Message);
}
