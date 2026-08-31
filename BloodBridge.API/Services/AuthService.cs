using BloodBridge.API.Data;
using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Services;

public sealed class AuthService
{
    private readonly BloodBridgeDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(
        BloodBridgeDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public Task<RegistrationResult> RegisterDonorAsync(RegisterDonorDto input) =>
        RegisterAsync(input.Email, input.Password, ApplicationRoles.Donor, async user =>
        {
            if (!BloodCompatibilityService.IsKnownBloodGroupValue(input.BloodGroup))
            {
                return "The specified BloodGroup is invalid.";
            }

            _context.Donors.Add(new Donor
            {
                UserId = user.Id,
                Name = input.Name.Trim(),
                BloodGroup = input.BloodGroup.Trim().ToUpperInvariant(),
                Phone = input.Phone.Trim(),
                Location = input.Location.Trim(),
                IsAvailable = input.IsAvailable
            });
            return null;
        });

    public Task<RegistrationResult> RegisterHospitalAsync(RegisterHospitalDto input) =>
        RegisterAsync(input.Email, input.Password, ApplicationRoles.Hospital, user =>
        {
            _context.Hospitals.Add(new Hospital
            {
                UserId = user.Id,
                Name = input.HospitalName.Trim(),
                Location = input.Location.Trim(),
                Phone = input.ContactInfo.Trim()
            });
            return Task.FromResult<string?>(null);
        });

    public Task<RegistrationResult> RegisterRequesterAsync(RegisterRequesterDto input) =>
        RegisterAsync(input.Email, input.Password, ApplicationRoles.Requester, user =>
        {
            _context.Requesters.Add(new Requester
            {
                UserId = user.Id,
                FullName = input.FullName.Trim(),
                ContactNumber = input.ContactNumber.Trim()
            });
            return Task.FromResult<string?>(null);
        });

    public async Task<AuthResult> LoginAsync(LoginDto input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email.Trim());
        if (user is null
            || !user.IsActive
            || user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            || !await _userManager.CheckPasswordAsync(user, input.Password))
        {
            return AuthResult.Failure("Invalid email or password.");
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        return role is null
            ? AuthResult.Failure("The user has no assigned role.")
            : AuthResult.Success(user, role);
    }

    private async Task<RegistrationResult> RegisterAsync(
        string email,
        string password,
        string role,
        Func<ApplicationUser, Task<string?>> addProfile)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await transaction.RollbackAsync();
                return RegistrationResult.Failure($"The {role} role is not configured.");
            }

            var user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim()
            };

            var userResult = await _userManager.CreateAsync(user, password);
            if (!userResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return RegistrationResult.Failure(userResult.Errors.Select(error => error.Description));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return RegistrationResult.Failure(roleResult.Errors.Select(error => error.Description));
            }

            var profileError = await addProfile(user);
            if (profileError is not null)
            {
                await transaction.RollbackAsync();
                return RegistrationResult.Failure(profileError);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RegistrationResult.Success(user, role);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync();
            return RegistrationResult.Failure($"Registration could not be completed: {exception.GetBaseException().Message}");
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return RegistrationResult.Failure($"Registration could not be completed: {exception.Message}");
        }
    }
}

public sealed class RegistrationResult
{
    private RegistrationResult(bool succeeded, ApplicationUser? user, string? role, IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        User = user;
        Role = role;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public ApplicationUser? User { get; }
    public string? Role { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static RegistrationResult Success(ApplicationUser user, string role) =>
        new(true, user, role, Array.Empty<string>());

    public static RegistrationResult Failure(params string[] errors) =>
        new(false, null, null, errors);

    public static RegistrationResult Failure(IEnumerable<string> errors) =>
        new(false, null, null, errors.ToArray());
}

public sealed class AuthResult
{
    private AuthResult(bool succeeded, ApplicationUser? user, string? role, string? error)
    {
        Succeeded = succeeded;
        User = user;
        Role = role;
        Error = error;
    }

    public bool Succeeded { get; }
    public ApplicationUser? User { get; }
    public string? Role { get; }
    public string? Error { get; }

    public static AuthResult Success(ApplicationUser user, string role) => new(true, user, role, null);
    public static AuthResult Failure(string error) => new(false, null, null, error);
}
