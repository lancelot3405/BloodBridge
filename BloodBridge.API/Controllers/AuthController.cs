using BloodBridge.API.Dtos;
using BloodBridge.API.Models;
using BloodBridge.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodBridge.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtTokenService _tokenService;

    public AuthController(AuthService authService, JwtTokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("register/donor")]
    public async Task<ActionResult<AuthResponseDto>> RegisterDonor(RegisterDonorDto input)
    {
        var result = await _authService.RegisterDonorAsync(input);
        return result.Succeeded ? Ok(CreateResponse(result)) : BadRequest(Errors(result));
    }

    [HttpPost("register/hospital")]
    public async Task<ActionResult<AuthResponseDto>> RegisterHospital(RegisterHospitalDto input)
    {
        var result = await _authService.RegisterHospitalAsync(input);
        return result.Succeeded ? Ok(CreateResponse(result)) : BadRequest(Errors(result));
    }

    [HttpPost("register/requester")]
    public async Task<ActionResult<AuthResponseDto>> RegisterRequester(RegisterRequesterDto input)
    {
        var result = await _authService.RegisterRequesterAsync(input);
        return result.Succeeded ? Ok(CreateResponse(result)) : BadRequest(Errors(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto input)
    {
        var result = await _authService.LoginAsync(input);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(CreateResponse(result.User!, result.Role!));
    }

    private AuthResponseDto CreateResponse(RegistrationResult result) =>
        CreateResponse(result.User!, result.Role!);

    private AuthResponseDto CreateResponse(ApplicationUser user, string role)
    {
        var token = _tokenService.CreateToken(user, role);
        return new AuthResponseDto
        {
            Token = token.Token,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = role,
            ExpiresAt = token.ExpiresAt
        };
    }

    private static object Errors(RegistrationResult result) =>
        new { message = string.Join(" ", result.Errors) };
}
