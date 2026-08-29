using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BloodBridge.Web.ViewModels;

namespace BloodBridge.Web.Services;

public sealed class WebAuthSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessTokenStore _tokenStore;

    public WebAuthSession(IHttpContextAccessor httpContextAccessor, IAccessTokenStore tokenStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenStore = tokenStore;
    }

    public async Task SignInAsync(AuthResponseViewModel response)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context is available.");

        _tokenStore.StoreToken(response.Token, response.ExpiresAt);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, response.UserId),
            new Claim(ClaimTypes.Name, response.Email),
            new Claim(ClaimTypes.Email, response.Email),
            new Claim(ClaimTypes.Role, response.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    public async Task SignOutAsync()
    {
        _tokenStore.ClearToken();
        if (_httpContextAccessor.HttpContext is not null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
