using System.Net.Http.Json;
using Microsoft.JSInterop;
using shot_reminder_2.Contracts.Auth;

namespace shot_reminder_2.Client.Services;

public sealed class AuthService(HttpClient http, IJSRuntime js)
{
    private const string AccessTokenKey = "access_token";

    public event Action? AuthStateChanged;

    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
        => await js.InvokeAsync<string?>("localStorage.getItem", ct, AccessTokenKey);

    public async Task<bool> IsLoggedInAsync(CancellationToken ct = default)
        => !string.IsNullOrWhiteSpace(await GetAccessTokenAsync(ct));

    public async Task LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => new InvalidOperationException("Username or password is invalid."),
                System.Net.HttpStatusCode.InternalServerError => new InvalidOperationException("Server error. Please try again later."),
                _ => new InvalidOperationException($"Login failed ({(int)response.StatusCode}).")
            };
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
        if (auth is null || string.IsNullOrWhiteSpace(auth.AccessToken))
        {
            throw new InvalidOperationException("Login response did not include an access token.");
        }

        await js.InvokeVoidAsync("localStorage.setItem", ct, AccessTokenKey, auth.AccessToken);

        AuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", ct, AccessTokenKey);
        AuthStateChanged?.Invoke();
    }
}
