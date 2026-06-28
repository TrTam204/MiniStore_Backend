using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniStore.Data;
using MiniStore.DTOs.User;
using MiniStore.DTOs.Login;
using MiniStore.DTOs.Auth;
using MiniStore.Models;
using MiniStore.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MiniStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(AppDbContext context, IConfiguration configuration, IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Vui lòng nhập email.");
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Vui lòng nhập mật khẩu.");
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
            {
                return BadRequest("Email hoặc mật khẩu không đúng.");
            }
            if (user.PasswordHash != request.Password)
            {
                return BadRequest("Email hoặc mật khẩu không đúng.");
            }
            var role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;
            var token = GenerateJwtToken(user.Id, user.Email, role);
            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = role,
                Token = token
            };
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest("Vui lòng nhập họ tên.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Vui lòng nhập email.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Vui lòng nhập mật khẩu.");
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return BadRequest("Vui lòng nhập số điện thoại.");
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest("Vui lòng nhập địa chỉ.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest("Email đã được sử dụng.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
                Phone = request.Phone,
                Address = request.Address,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user.Id, user.Email, user.Role);
            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new
            {
                UserId = userId,
                Email = email,
                Message = "Token hợp lệ, bạn đã đăng nhập.",
                Role = role
            });
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(dto);
                return Ok(new { message = "Đã gửi mã OTP vào email, vui lòng kiểm tra hộp thư." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto dto)
        {
            var ok = await _authService.VerifyOtpAsync(dto);
            if (ok) return Ok(new { message = "OTP hợp lệ." });
            return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn." });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { message = "Đổi mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GenerateJwtToken(int userId, string email, string role)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiresInMinutes),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}