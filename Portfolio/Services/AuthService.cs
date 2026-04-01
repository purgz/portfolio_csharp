using System.Net.Http.Json;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;

namespace Portfolio.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private const string TokenKey = "auth_token";

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/login",
            new { username, password });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result!.Token);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (string.IsNullOrEmpty(token)) return false;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo > DateTime.UtcNow;
    }
}

public record TokenResponse(string Token);