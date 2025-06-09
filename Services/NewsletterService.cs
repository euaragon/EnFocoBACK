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
            return await _context.Newsletters
                .OrderByDescending(n => n.Id)
                .ToListAsync();
        }

        public async Task AddSubscriber(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío.");

            // Verificamos si ya existe
            bool exists = await _context.Newsletters.AnyAsync(n => n.Email == email);

            if (exists)
                throw new InvalidOperationException("Este email ya está suscrito.");

            var subscriber = new Newsletter
            {
                Email = email
            };

            _context.Newsletters.Add(subscriber);
            await _context.SaveChangesAsync();
        }
    }
}
