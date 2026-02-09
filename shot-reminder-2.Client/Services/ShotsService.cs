using shot_reminder_2.Contracts.Common;
using shot_reminder_2.Contracts.Shots;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace shot_reminder_2.Client.Services;

public sealed class ShotsService(HttpClient http, AuthService auth)
{
    public async Task<RegisterShotResponse> RegisterShotAsync(RegisterShotRequest request, CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/shots")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Register shot failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterShotResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task<ShotItemDto> GetLatestShotAsync(CancellationToken ct = default)
    {
        var token = await auth.GetAccessTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/shots/latest");

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Get latest shot failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<ShotItemDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task<PagedResponse<ShotItemDto>> GetAllShotsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

        var token = await auth.GetAccessTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Not logged in.");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/shots?page={page}&pageSize={pageSize}");

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await auth.LogoutAsync(ct);

            throw await CreateApiExceptionAsync(response, "Get all shots failed", ct);
        }

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ShotItemDto>>(cancellationToken: ct);
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
