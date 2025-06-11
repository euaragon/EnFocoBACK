using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnFoco_new.Models;
using EnFoco_new.Services;
using Microsoft.Extensions.Logging;

namespace EnFoco_new.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger; // Declara la instancia de ILogger

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger; // Inyecta ILogger en el constructor
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Log del intento de inicio de sesión
            _logger.LogInformation("Intento de inicio de sesión para el usuario: {UserName}", request.Name);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Intento de inicio de sesión fallido para el usuario '{UserName}': Nombre de usuario o contraseña vacíos.", request.Name);
                return BadRequest("Nombre de usuario y contraseña son requeridos.");
            }

            var user = await _userService.GetUserByNameAsync(request.Name);

            if (user == null || !await _userService.VerifyPasswordAsync(user, request.Password))
            {
                // Log de inicio de sesión fallido (credenciales incorrectas)
                _logger.LogWarning("Inicio de sesión fallido para el usuario '{UserName}': Credenciales incorrectas.", request.Name);
                return Unauthorized("Credenciales incorrectas.");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Name ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                        // Puedes agregar más claims según tus necesidades
                    }),
                    Expires = DateTime.UtcNow.AddHours(6),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Log de inicio de sesión exitoso
                _logger.LogInformation("Inicio de sesión exitoso para el usuario '{UserName}' (ID: {UserId}).", user.Name, user.Id);
                return Ok(new { Token = tokenString });
            }
            catch (Exception ex)
            {
                // Log de cualquier error inesperado durante la generación del token
                _logger.LogError(ex, "Error inesperado al generar token JWT para el usuario '{UserName}'.", request.Name);
                return StatusCode(500, "Error interno del servidor al procesar el inicio de sesión.");
            }
        }
    }

    public class LoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}