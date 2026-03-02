using System.Net.Http.Headers;
using System.Net.Http.Json;
using shot_reminder_2.Contracts.Google;

namespace shot_reminder_2.Client.Services;

public sealed class GoogleCalendarConnectionService(HttpClient http, AuthService auth)
{
    public async Task<GoogleConnectionStatusResponse> GetStatusAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/google/status");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Get Google status failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleConnectionStatusResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task<string> GetConnectUrlAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/google/connect");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Google connect failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleConnectResponse>(cancellationToken: ct);
        return result?.Url ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/google/disconnect");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Google disconnect failed", ct);
        }
    }

    private static async Task<Exception> CreateApiExceptionAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return new InvalidOperationException("Your session has expired. Please log in again.");

        string? details = null;
        try { details = await response.Content.ReadAsStringAsync(ct); } catch { }

        return new InvalidOperationException(string.IsNullOrWhiteSpace(details)
            ? $"{operation} ({(int)response.StatusCode})."
            : details);
    }
}
