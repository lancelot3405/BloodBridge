using BloodBridge.Web.Services;
using BloodBridge.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.Web.Controllers;

[AllowAnonymous]
public sealed class AuthController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly WebAuthSession _session;

    public AuthController(ApiClient apiClient, WebAuthSession session)
    {
        _apiClient = apiClient;
        _session = session;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiClient.PostAsync<AuthResponseViewModel>("api/auth/login", model);
            await _session.SignInAsync(response);
            return LocalRedirectOrDashboard(model.ReturnUrl, response.Role);
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ValidateRoleFields(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var endpoint = model.Role switch
        {
            "Donor" => "api/auth/register/donor",
            "Requester" => "api/auth/register/requester",
            "Hospital" => "api/auth/register/hospital",
            _ => string.Empty
        };

        object payload = model.Role switch
        {
            "Donor" => new { model.Email, model.Password, model.Name, model.BloodGroup, model.Phone, model.Location, model.IsAvailable },
            "Requester" => new { model.Email, model.Password, model.FullName, model.ContactNumber },
            "Hospital" => new { model.Email, model.Password, model.HospitalName, Location = model.Location, ContactInfo = model.ContactInfo },
            _ => new { }
        };

        try
        {
            var response = await _apiClient.PostAsync<AuthResponseViewModel>(endpoint, payload);
            await _session.SignInAsync(response);
            return RedirectToDashboard(response.Role);
        }
        catch (ApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _session.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private void ValidateRoleFields(RegisterViewModel model)
    {
        if (model.Role is not ("Donor" or "Requester" or "Hospital"))
        {
            ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
            return;
        }

        if (model.Role == "Donor")
        {
            Require(model.Name, nameof(model.Name), "Name is required.");
            Require(model.BloodGroup, nameof(model.BloodGroup), "Blood group is required.");
            Require(model.Phone, nameof(model.Phone), "Phone is required.");
            Require(model.Location, nameof(model.Location), "Location is required.");
        }
        else if (model.Role == "Requester")
        {
            Require(model.FullName, nameof(model.FullName), "Full name is required.");
            Require(model.ContactNumber, nameof(model.ContactNumber), "Contact number is required.");
        }
        else
        {
            Require(model.HospitalName, nameof(model.HospitalName), "Hospital name is required.");
            Require(model.Location, nameof(model.Location), "Location is required.");
            Require(model.ContactInfo, nameof(model.ContactInfo), "Contact information is required.");
        }
    }

    private void Require(string? value, string key, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ModelState.AddModelError(key, message);
        }
    }

    private IActionResult LocalRedirectOrDashboard(string? returnUrl, string role) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToDashboard(role);

    private IActionResult RedirectToDashboard(string role) => role switch
    {
        "Donor" => RedirectToAction("Index", "Donor")!,
        "Hospital" => RedirectToAction("Index", "Hospital")!,
        "Requester" => RedirectToAction("Index", "Requester")!,
        _ => RedirectToAction("Index", "Home")!
    };
}
