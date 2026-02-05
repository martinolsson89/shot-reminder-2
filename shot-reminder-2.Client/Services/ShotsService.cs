using System.Net.Http.Headers;
using System.Net.Http.Json;
using shot_reminder_2.Contracts.Shots;

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
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new InvalidOperationException("Your session has expired. Please log in again."),
                System.Net.HttpStatusCode.BadRequest => new InvalidOperationException("Please verify your input and try again."),
                _ => new InvalidOperationException($"Register shot failed ({(int)response.StatusCode}).")
            };
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterShotResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Server returned an empty response.");
    }
}
