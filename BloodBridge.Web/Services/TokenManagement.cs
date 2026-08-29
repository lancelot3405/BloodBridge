using System.Net.Http.Headers;

namespace BloodBridge.Web.Services;

public interface IAccessTokenStore
{
    string? GetToken();
    void StoreToken(string token, DateTime expiresAt);
    void ClearToken();
}

public sealed class CookieAccessTokenStore : IAccessTokenStore
{
    private const string CookieName = "BloodBridge.Web.AccessToken";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieAccessTokenStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken() =>
        _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];

    public void StoreToken(string token, DateTime expiresAt)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context is available.");

        context.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiresAt.ToUniversalTime()),
            IsEssential = true
        });
    }

    public void ClearToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
    }
}

public sealed class JwtDelegatingHandler : DelegatingHandler
{
    private readonly IAccessTokenStore _tokenStore;

    public JwtDelegatingHandler(IAccessTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
