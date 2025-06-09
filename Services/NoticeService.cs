using EnFoco_new.Data;
using EnFoco_new.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnFoco_new.Services
{
    public class NoticeService : INoticeService
    {
        private readonly EnFocoDb _context;
        private readonly ILogger<NoticeService> _logger;

        public NoticeService(EnFocoDb context, ILogger<NoticeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Notice>> GetAllNoticesAsync()
        {
            return await _context.Notices
                .OrderByDescending(n => n.Id)
                .ToListAsync();
        }

        public IList<Notice> GetNotice() => _context.Notices.ToList();

        public IList<Newsletter> GetNewsletter() => _context.Newsletters.ToList();

        public async Task<Notice?> GetNoticeByIdAsync(int id)
        {
            return await _context.Notices.FindAsync(id);
        }



        public async Task AddNoticeAsync(Notice notice)
        {
            if (notice == null)
                throw new ArgumentNullException(nameof(notice));

            try
            {
                _context.Notices.Add(notice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar la noticia.");
                throw;
            }
        }

        public IList<Notice> SearchNotices(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<Notice>();

                searchTerm = searchTerm.ToLower();

                return _context.Notices
                    .Where(notice =>
                        (notice.Title != null && notice.Title.ToLower().Contains(searchTerm)) ||
                        (notice.Text != null && notice.Text.ToLower().Contains(searchTerm)))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la búsqueda: {ex.Message}");
                return new List<Notice>();
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
            if (_context.Entry(notice).State == EntityState.Detached)
            {
                _context.Attach(notice);
            }

            _context.Notices.Update(notice);
            await _context.SaveChangesAsync();
        }

        public async Task<IList<Notice>> GetPaginatedNoticesAsync(int page, int pageSize, string? searchTerm = null, string? category = null)
        {
            IQueryable<Notice> query = _context.Notices;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm.ToLower())) || (n.Text != null && n.Text.ToLower().Contains(searchTerm.ToLower())));
            }

            if (!string.IsNullOrEmpty(category))
            {
                var loweredCategory = category.ToLowerInvariant();
                query = query.Where(n => n.Category.ToString().ToLowerInvariant() == loweredCategory);
            }

            // Ejecutamos la consulta sin paginar primero (esto se hace en la base)
            var allResults = await query
                .OrderByDescending(n => n.Id)
                .ToListAsync();

            // Luego paginamos en memoria (evita OFFSET/FETCH)
            var paginatedResults = allResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return paginatedResults;
        }

        public async Task<int> GetTotalNoticesCountAsync(string? searchTerm = null, string? category = null)
        {
            IQueryable<Notice> query = _context.Notices;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm.ToLower())) || (n.Text != null && n.Text.ToLower().Contains(searchTerm.ToLower())));
            }

            if (!string.IsNullOrEmpty(category))
            {
                var loweredCategory = category.ToLowerInvariant();
                query = query.Where(n => n.Category.ToString().ToLowerInvariant() == loweredCategory);
            }

            return await query.CountAsync();
        }
    }
}
