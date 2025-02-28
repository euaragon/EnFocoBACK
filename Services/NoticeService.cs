using EnFoco_new.Data;
using EnFoco_new.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnFoco_new.Services
{
    public class NoticeService
    {
        private readonly EnFocoDb _context;
        private readonly ILogger<NoticeService> _logger; // El logger

        public NoticeService(EnFocoDb context, ILogger<NoticeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Notice> GetNotice()
        {
            return _context.Notices.ToList();
        }

        public IList<Newsletter> GetNewsletter()
        {
            return _context.Newsletters.ToList();
        }

        public async Task<Notice?> GetNoticeByIdAsync(int id)
        {
            var notice = await _context.Notices.FindAsync(id);
            return notice;
        }

        public async Task AddNoticeAsync(Notice notice)
        {
            if (notice == null)
            {
                throw new ArgumentNullException(nameof(notice)); // Lanza una excepción si el objeto es nulo
            }

            try
            {
                _context.Notices.Add(notice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log del error (usando un logger adecuado)
                _logger.LogError(ex, "Error al agregar la noticia.");

                // Opcional: Relanzar la excepción para que el controlador la maneje
                throw; // o throw new Exception("Error al guardar la noticia", ex);
            }
        }

        public IList<Notice> SearchNotices(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<Notice>();
                }

                searchTerm = searchTerm.ToLower();
                var searchResults = _context.Notices
                    .Where(notice =>
                        (notice.Title != null && notice.Title.ToLower().Contains(searchTerm)) ||
                        (notice.Text != null && notice.Text.ToLower().Contains(searchTerm))
                    )
                    .ToList();

                return searchResults;
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar el error usando algún sistema de logging
                Console.WriteLine($"Error en la búsqueda: {ex.Message}");
                return new List<Notice>(); // Retorna una lista vacía en caso de error
            }
        }



        public async Task DeleteNoticeAsync(int id)
        {
            var notice = await _context.Notices.FindAsync(id);
            if (notice != null)
            {
                _context.Notices.Remove(notice);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateNoticeAsync(Notice notice)
        {
            // 1.  Verificación (opcional pero recomendada):  Asegura que la entidad no esté en estado Detached.
            if (_context.Entry(notice).State == EntityState.Detached)
            {
                _context.Attach(notice); // Si está Detached, adjúntala para que EF Core la rastree
            }

            // 2.  No es necesario buscar la entidad. EF Core ya rastrea los cambios.
            _context.Notices.Update(notice); // Actualiza la entidad (EF Core ya sabe qué cambiar)

            // 3.  Guarda los cambios.
            await _context.SaveChangesAsync();
        }


    }
}
