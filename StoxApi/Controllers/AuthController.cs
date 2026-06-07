using Microsoft.AspNetCore.Mvc;

namespace StockApi.Controllers;

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
        var user = _authService.ValidUserLogin(request);
        if (user != null)
        {
            var token = _authService.GenerateJwtToken(user);
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