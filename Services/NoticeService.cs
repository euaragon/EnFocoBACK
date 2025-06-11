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
            _logger.LogInformation("Inicio de GetAllNoticesAsync: Obteniendo todas las noticias de la base de datos.");
            try
            {
                var notices = await _context.Notices
                    .OrderByDescending(n => n.Id)
                    .ToListAsync();
                _logger.LogInformation("Fin de GetAllNoticesAsync: {NoticeCount} noticias obtenidas exitosamente.", notices.Count);
                return notices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllNoticesAsync: Falló al obtener todas las noticias.");
                throw; // Re-lanza la excepción para que el controlador la maneje.
            }
        }









        public IList<Notice> GetNotice()
        {
            _logger.LogInformation("Inicio de GetNotice: Obteniendo todas las noticias (IList).");
            try
            {
                var notices = _context.Notices.ToList();
                _logger.LogInformation("Fin de GetNotice: {NoticeCount} noticias obtenidas (IList).", notices.Count);
                return notices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetNotice: Falló al obtener noticias (IList).");
                throw;
            }
        }







        public IList<Newsletter> GetNewsletter()
        {
            _logger.LogInformation("Inicio de GetNewsletter: Obteniendo todas las suscripciones a la newsletter.");
            try
            {
                var newsletters = _context.Newsletters.ToList();
                _logger.LogInformation("Fin de GetNewsletter: {NewsletterCount} suscripciones obtenidas.", newsletters.Count);
                return newsletters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetNewsletter: Falló al obtener las suscripciones a la newsletter.");
                throw;
            }
        }







        public async Task<Notice?> GetNoticeByIdAsync(int id)
        {
            _logger.LogInformation("Inicio de GetNoticeByIdAsync: Buscando noticia con ID: {NoticeId}.", id);
            try
            {
                var notice = await _context.Notices.FindAsync(id);
                if (notice == null)
                {
                    _logger.LogWarning("Fin de GetNoticeByIdAsync: Noticia con ID {NoticeId} no encontrada.", id);
                }
                else
                {
                    _logger.LogInformation("Fin de GetNoticeByIdAsync: Noticia con ID {NoticeId} encontrada.", id);
                }
                return notice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetNoticeByIdAsync: Falló al buscar noticia con ID {NoticeId}.", id);
                throw;
            }
        }





        public async Task AddNoticeAsync(Notice notice)
        {
            _logger.LogInformation("Inicio de AddNoticeAsync: Agregando nueva noticia con título: {NoticeTitle}.", notice?.Title);
            if (notice == null)
            {
                _logger.LogError("Error en AddNoticeAsync: Se intentó agregar una noticia nula.");
                throw new ArgumentNullException(nameof(notice));
            }

            try
            {
                _context.Notices.Add(notice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Fin de AddNoticeAsync: Noticia '{NoticeTitle}' (ID: {NoticeId}) agregada exitosamente.", notice.Title, notice.Id);
            }
            catch (DbUpdateException ex) // Captura excepciones específicas de DB si es posible
            {
                _logger.LogError(ex, "Error en AddNoticeAsync: Falló al guardar la noticia '{NoticeTitle}' en la base de datos.", notice.Title);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en AddNoticeAsync: Falló al agregar la noticia '{NoticeTitle}'.", notice.Title);
                throw;
            }
        }




        public IList<Notice> SearchNotices(string searchTerm)
        {
            _logger.LogInformation("Inicio de SearchNotices: Buscando noticias con término: '{SearchTerm}'.", searchTerm);
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Fin de SearchNotices: Término de búsqueda vacío o nulo. Retornando lista vacía.");
                    return new List<Notice>();
                }

                searchTerm = searchTerm.ToLower();

                var results = _context.Notices
                    .Where(notice =>
                        (notice.Title != null && notice.Title.ToLower().Contains(searchTerm)) ||
                        (notice.Text != null && notice.Text.ToLower().Contains(searchTerm)))
                    .ToList();

                _logger.LogInformation("Fin de SearchNotices: Se encontraron {ResultCount} resultados para '{SearchTerm}'.", results.Count, searchTerm);
                return results;
            }
            catch (Exception ex)
            {
                // Reemplazado Console.WriteLine con _logger.LogError
                _logger.LogError(ex, "Error en SearchNotices: Falló al realizar la búsqueda para '{SearchTerm}'.", searchTerm);
                throw; // Es mejor relanzar para que el controlador pueda manejar el error de la API
            }
        }





        public async Task DeleteNoticeAsync(int id)
        {
            _logger.LogInformation("Inicio de DeleteNoticeAsync: Intentando eliminar noticia con ID: {NoticeId}.", id);
            try
            {
                var notice = await _context.Notices.FindAsync(id);
                if (notice != null)
                {
                    _context.Notices.Remove(notice);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Fin de DeleteNoticeAsync: Noticia con ID {NoticeId} eliminada exitosamente.", id);
                }
                else
                {
                    _logger.LogWarning("Fin de DeleteNoticeAsync: Noticia con ID {NoticeId} no encontrada para eliminación.", id);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error en DeleteNoticeAsync: Falló al eliminar noticia con ID {NoticeId} de la base de datos.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en DeleteNoticeAsync: Falló al eliminar noticia con ID {NoticeId}.", id);
                throw;
            }
        }






        public async Task UpdateNoticeAsync(Notice notice)
        {
            _logger.LogInformation("Inicio de UpdateNoticeAsync: Intentando actualizar noticia con ID: {NoticeId}.", notice?.Id);
            if (notice == null)
            {
                _logger.LogError("Error en UpdateNoticeAsync: Se intentó actualizar una noticia nula.");
                throw new ArgumentNullException(nameof(notice));
            }

            try
            {
                if (_context.Entry(notice).State == EntityState.Detached)
                {
                    _context.Attach(notice);
                    _logger.LogDebug("Noticia con ID {NoticeId} adjuntada al contexto para actualización.", notice.Id);
                }

                _context.Notices.Update(notice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Fin de UpdateNoticeAsync: Noticia con ID {NoticeId} actualizada exitosamente.", notice.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Error de concurrencia al actualizar noticia con ID {NoticeId}.", notice.Id);
                throw; // Re-lanza la excepción para que el controlador la maneje.
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error en UpdateNoticeAsync: Falló al guardar la actualización de noticia con ID {NoticeId} en la base de datos.", notice.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en UpdateNoticeAsync: Falló al actualizar noticia con ID {NoticeId}.", notice.Id);
                throw;
            }
        }





        public async Task<IList<Notice>> GetPaginatedNoticesAsync(int page, int pageSize, string? searchTerm = null, string? category = null)
        {
            _logger.LogInformation("Inicio de GetPaginatedNoticesAsync: Obteniendo noticias paginadas. Página: {Page}, Tamaño: {PageSize}, Término: '{SearchTerm}', Categoría: '{Category}'.", page, pageSize, searchTerm, category);
            try
            {
                IQueryable<Notice> query = _context.Notices;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm.ToLower())) || (n.Text != null && n.Text.ToLower().Contains(searchTerm.ToLower())));
                    _logger.LogDebug("Filtro de búsqueda '{SearchTerm}' aplicado a la consulta.", searchTerm);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    var loweredCategory = category.ToLowerInvariant();
                    query = query.Where(n => n.Category.ToString().ToLowerInvariant() == loweredCategory);
                    _logger.LogDebug("Filtro de categoría '{Category}' aplicado a la consulta.", category);
                }

                var allResults = await query
                    .OrderByDescending(n => n.Id)
                    .ToListAsync();
                _logger.LogDebug("Total de resultados sin paginar para consulta paginada: {TotalResults}.", allResults.Count);

                var paginatedResults = allResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Fin de GetPaginatedNoticesAsync: {ResultCount} noticias obtenidas para página {Page}.", paginatedResults.Count, page);
                return paginatedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetPaginatedNoticesAsync: Falló al obtener noticias paginadas para página {Page}, término '{SearchTerm}', categoría '{Category}'.", page, searchTerm, category);
                throw;
            }
        }





        public async Task<int> GetTotalNoticesCountAsync(string? searchTerm = null, string? category = null)
        {
            _logger.LogInformation("Inicio de GetTotalNoticesCountAsync: Calculando el conteo total de noticias. Término: '{SearchTerm}', Categoría: '{Category}'.", searchTerm, category);
            try
            {
                IQueryable<Notice> query = _context.Notices;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm.ToLower())) || (n.Text != null && n.Text.ToLower().Contains(searchTerm.ToLower())));
                    _logger.LogDebug("Filtro de búsqueda '{SearchTerm}' aplicado al conteo.", searchTerm);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    var loweredCategory = category.ToLowerInvariant();
                    query = query.Where(n => n.Category.ToString().ToLowerInvariant() == loweredCategory);
                    _logger.LogDebug("Filtro de categoría '{Category}' aplicado al conteo.", category);
                }

                var count = await query.CountAsync();
                _logger.LogInformation("Fin de GetTotalNoticesCountAsync: Conteo total de noticias: {Count}.", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetTotalNoticesCountAsync: Falló al calcular el conteo total de noticias para término '{SearchTerm}', categoría '{Category}'.", searchTerm, category);
                throw;
            }
        }
    }
}
