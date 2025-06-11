using EnFoco_new.Data;
using EnFoco_new.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnFoco_new.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly EnFocoDb _context;
        private readonly ILogger<NewsletterService> _logger;

        public NewsletterService(EnFocoDb context, ILogger<NewsletterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Newsletter>> GetAllNewsletter()
        {
            _logger.LogInformation("Inicio de GetAllNewsletter: Obteniendo todas las suscripciones a la newsletter.");
            try
            {
                var newsletters = await _context.Newsletters
                    .OrderByDescending(n => n.Id)
                    .ToListAsync();
                _logger.LogInformation("Fin de GetAllNewsletter: Se obtuvieron {SubscriberCount} suscripciones.", newsletters.Count);
                return newsletters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllNewsletter: Falló al obtener las suscripciones a la newsletter.");
                throw; // Re-lanza la excepción para manejo en la capa superior
            }
        }







        public async Task AddSubscriber(string email)
        {
            _logger.LogInformation("Inicio de AddSubscriber: Intentando suscribir el email: {Email}", email);
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Fin de AddSubscriber: Intento de suscripción con email vacío o nulo.");
                    throw new ArgumentException("El email no puede estar vacío.");
                }

                // Verificamos si ya existe
                bool exists = await _context.Newsletters.AnyAsync(n => n.Email == email);

                if (exists)
                {
                    _logger.LogWarning("Fin de AddSubscriber: El email '{Email}' ya está suscrito. No se agregó duplicado.", email);
                    throw new InvalidOperationException("Este email ya está suscrito.");
                }

                var subscriber = new Newsletter
                {
                    Email = email
                };

                _context.Newsletters.Add(subscriber);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fin de AddSubscriber: Email '{Email}' suscrito exitosamente (ID: {SubscriberId}).", email, subscriber.Id);
            }
            catch (ArgumentException ex) // Para emails vacíos/nulos
            {
                _logger.LogWarning(ex, "Error en AddSubscriber: El email proporcionado es inválido. Mensaje: {ErrorMessage}", ex.Message);
                throw; // Re-lanza la excepción
            }
            catch (InvalidOperationException ex) // Para emails ya existentes
            {
                _logger.LogWarning(ex, "Error en AddSubscriber: Email '{Email}' ya existe en las suscripciones. Mensaje: {ErrorMessage}", email, ex.Message);
                throw; // Re-lanza la excepción
            }
            catch (DbUpdateException ex) // Para errores de base de datos
            {
                _logger.LogError(ex, "Error en AddSubscriber: Falló al guardar el email '{Email}' en la base de datos.", email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en AddSubscriber: Falló la suscripción del email '{Email}'.", email);
                throw;
            }
        }
    }
}
