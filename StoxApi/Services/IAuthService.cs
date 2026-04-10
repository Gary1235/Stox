using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StoxApi.Models.Stox;

public interface IAuthService
{
    public string GenerateJwtToken(string username);

    public bool ValidUserLogin(LoginDto dto);

    public SaveChangeResult CreateAdmin();
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IConfiguration config, IUnitOfWork unitOfWork)
    {
        _config = config;
        _unitOfWork = unitOfWork;
    }

    public string GenerateJwtToken(string username)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 設定 Token 內含的資訊 (Claims)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3), // Token 3 小時後過期 (過渡期 ， 最後要開發雙token)
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidUserLogin(LoginDto dto)
    {
        var isValid = false;

        isValid = _unitOfWork.GetRepository<User>().Where(x => x.Account == dto.Username && x.Password == dto.Password).Any();

        return isValid;
    }

    public SaveChangeResult CreateAdmin()
    {
        var model = new User
        {
            Id = Guid.CreateVersion7(),
            Account = "admin",
            Password = "000000",
            Email = "gary19961221@gmail.com",
            CreatedDate = DateTime.Now,
        };

        _unitOfWork.GetRepository<User>().Add(model);
        var result = _unitOfWork.SaveChanges();

        return result.IsSuccess ? SaveChangeResult.Success("成功") : SaveChangeResult.Failure("發生錯誤");
    }
}