using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StoxApi.Models.Stox;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto request)
    {
        // 驗證帳號&密碼
        if (_authService.ValidUserLogin(request))
        {
            var token = _authService.GenerateJwtToken(request.Username);
            return Ok(new { Token = token });
        }
        return Unauthorized("帳號或密碼錯誤");
    }

    

    [HttpPost("CreateAdmin")]
    public IActionResult CreateAdmin()
    {
        var result = _authService.CreateAdmin();

        return Ok(result.Message);
    }
}