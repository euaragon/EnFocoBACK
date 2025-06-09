using EnFoco_new.Models;
using EnFoco_new.Services;
using Microsoft.AspNetCore.Mvc;

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
            var newsletters = await _newsletterService.GetAllNewsletter();
            return Ok(newsletters);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("El email es obligatorio.");

            try
            {
                await _newsletterService.AddSubscriber(request.Email);
                return Ok(new { message = "Suscripción exitosa." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al suscribirse");
                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}

public class SubscribeRequest
{
    public string? Email { get; set; }
}