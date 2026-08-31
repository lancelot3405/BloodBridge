using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Identity;

namespace BloodBridge.API.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSuccess(roleResult, $"Unable to seed role {role}");
            }
        }

        var email = configuration["Admin:Email"]?.Trim();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Admin:Email and Admin:Password must be configured for default administrator seeding.");
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);
            EnsureSuccess(createResult, "Unable to create the default administrator");
        }
        else if (!admin.IsActive)
        {
            admin.IsActive = true;
            var activateResult = await userManager.UpdateAsync(admin);
            EnsureSuccess(activateResult, "Unable to activate the default administrator");
        }

        if (!await userManager.IsInRoleAsync(admin, ApplicationRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
            EnsureSuccess(roleResult, "Unable to assign the Admin role to the default administrator");
        }
    }

    private static void EnsureSuccess(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message}: {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }
}
