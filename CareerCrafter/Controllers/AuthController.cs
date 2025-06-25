using CareerCrafter.DTOs;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace CareerCrafter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private static readonly ILog _log = LogManager.GetLogger(typeof(AuthController));

        public AuthController(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            _log.Info($"[Register] Attempt by: {dto.Email}");

            try
            {
                // ✅ Basic email validation
                if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@") || !dto.Email.Contains("."))
                {
                    _log.Warn($"[Register] Invalid email format: {dto.Email}");
                    return BadRequest("Invalid email format.");
                }

                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                {
                    _log.Warn("[Register] Weak password");
                    return BadRequest("Password must be at least 6 characters.");
                }

                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _log.Warn($"[Register] Duplicate email: {dto.Email}");
                    return BadRequest("User already exists with this email.");
                }

                var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(dto.Password)));

                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    Role = dto.Role
                };

                await _userRepository.RegisterUserAsync(user);

                _log.Info($"[Register] Success for: {dto.Email}");
                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                _log.Error($"[Register] Error for: {dto.Email}", ex);
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            _log.Info($"[Login] Attempt by: {dto.Email}");

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    _log.Warn($"[Login] Invalid email: {dto.Email}");
                    return Unauthorized("Invalid credentials.");
                }

                var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(dto.Password)));

                if (user.PasswordHash != hash)
                {
                    _log.Warn($"[Login] Wrong password for: {dto.Email}");
                    return Unauthorized("Invalid credentials.");
                }

                var token = GenerateJwtToken(user);

                _log.Info($"[Login] Success for: {dto.Email}");
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _log.Error($"[Login] Error for: {dto.Email}", ex);
                return StatusCode(500, "An error occurred during login.");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
