using EnFoco_new.Models;
using EnFoco_new.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnFoco_new.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : ControllerBase
    {
        private readonly ILogger<NewsletterController> _logger;
        private readonly INewsletterService _newsletterService;

        public NewsletterController(ILogger<NewsletterController> logger, INewsletterService newsletterService)
        {
            _logger = logger;
            _newsletterService = newsletterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Inicio de GetAll: Solicitando todas las suscripciones a la newsletter.");
            try
            {
                var newsletters = await _newsletterService.GetAllNewsletter();
                _logger.LogInformation("Fin de GetAll: {SubscriptionCount} suscripciones a la newsletter obtenidas exitosamente.", newsletters.Count());
                return Ok(newsletters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAll: No se pudieron obtener las suscripciones a la newsletter.");
                return StatusCode(500, "Error interno del servidor al obtener las suscripciones.");
            }
        }






        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            _logger.LogInformation("Inicio de Subscribe: Intento de suscripción con email: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Fin de Subscribe: Intento de suscripción fallido. El email es obligatorio.");
                return BadRequest("El email es obligatorio.");
            }

            try
            {
                await _newsletterService.AddSubscriber(request.Email);
                _logger.LogInformation("Fin de Subscribe: Email '{Email}' suscrito exitosamente a la newsletter.", request.Email);
                return Ok(new { message = "Suscripción exitosa." });
            }
            catch (InvalidOperationException ex)
            {
                // Este catch es específico para emails ya existentes (asumiendo que NewsletterService lanza InvalidOperationException)
                _logger.LogWarning(ex, "Fin de Subscribe: Intento de suscripción con email '{Email}' fallido. El email ya está suscrito. Mensaje: {ErrorMessage}", request.Email, ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Este catch es para cualquier otro error inesperado
                _logger.LogError(ex, "Error al suscribir el email '{Email}' a la newsletter.", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}

public class SubscribeRequest
{
    public string? Email { get; set; }
}