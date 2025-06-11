using EnFoco_new.Data; 
using EnFoco_new.Models; 
using EnFoco_new.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;


namespace EnFoco_new.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoticeController : ControllerBase
    {
        private readonly ILogger<NoticeController> _logger;
        private readonly INoticeService _noticeService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EnFocoDb _context;

        public NoticeController(ILogger<NoticeController> logger, INoticeService noticeService, IWebHostEnvironment webHostEnvironment, EnFocoDb context)
        {
            _logger = logger;
            _noticeService = noticeService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Inicio de GetAll: Obteniendo todas las noticias."); // Log al inicio de la operación
            try
            {
                var notices = await _noticeService.GetAllNoticesAsync();
                _logger.LogInformation("Fin de GetAll: {NoticeCount} noticias obtenidas exitosamente.", notices.Count()); // Log al éxito
                return Ok(notices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAll: No se pudieron obtener las noticias."); // Log del error
                return StatusCode(500, "Error interno del servidor al obtener las noticias.");
            }
        }





        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Inicio de GetById: Buscando noticia con ID: {NoticeId}.", id);
            try
            {
                var notice = await _noticeService.GetNoticeByIdAsync(id);
                if (notice == null)
                {
                    _logger.LogWarning("Fin de GetById: Noticia con ID {NoticeId} no encontrada.", id); // Log para casos de no encontrado
                    return NotFound();
                }
                _logger.LogInformation("Fin de GetById: Noticia con ID {NoticeId} obtenida exitosamente.", id);
                return Ok(notice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetById: No se pudo obtener la noticia con ID {NoticeId}.", id);
                return StatusCode(500, "Error interno del servidor al obtener la noticia.");
            }
        }






        [HttpGet("section/{section}")]
        public IActionResult GetBySection(string section)
        {
            _logger.LogInformation("Inicio de GetBySection: Buscando noticias en la sección: {Section}.", section);
            if (!Enum.TryParse(section, out NoticeSection parsedSection))
            {
                _logger.LogWarning("Fin de GetBySection: Solicitud de sección inválida: {Section}.", section);
                return BadRequest("Sección inválida.");
            }

            try
            {
                var notices = _noticeService.GetNotice()
                    .Where(n => n.Section == parsedSection)
                    .OrderByDescending(n => n.Id)
                    .ToList();

                _logger.LogInformation("Fin de GetBySection: Se obtuvieron {NoticeCount} noticias para la sección {Section}.", notices.Count, parsedSection);
                return Ok(notices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetBySection: No se pudieron obtener noticias para la sección {Section}.", section);
                return StatusCode(500, "Error interno del servidor al obtener noticias por sección.");
            }
        }










        [HttpGet("search")]
        public IActionResult Search(string searchTerm, int page = 1, int pageSize = 6)
        {
            _logger.LogInformation("Inicio de Search: Buscando '{SearchTerm}' en página {Page} con tamaño {PageSize}.", searchTerm, page, pageSize);
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogWarning("Fin de Search: Término de búsqueda vacío o nulo.");
                return BadRequest("Término de búsqueda requerido.");
            }

            try
            {
                var query = _noticeService.SearchNotices(searchTerm).AsQueryable();
                var totalCount = query.Count();
                var notices = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                _logger.LogInformation("Fin de Search: Se encontraron {TotalCount} resultados para '{SearchTerm}'. Páginas: {TotalPages}.", totalCount, searchTerm, (int)Math.Ceiling((double)totalCount / pageSize));
                return Ok(new PagedResult<Notice>
                {
                    Data = notices,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Search: No se pudo realizar la búsqueda para '{SearchTerm}'.", searchTerm);
                return StatusCode(500, "Error interno del servidor al realizar la búsqueda.");
            }
        }






        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Notice notice)
        {
            _logger.LogInformation("Inicio de Create: Intentando crear nueva noticia.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Fin de Create: Fallo de validación del modelo al crear noticia. Errores: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            if (notice.ImageFile == null || notice.ImageFile.Length == 0)
            {
                _logger.LogWarning("Fin de Create: Imagen requerida para crear noticia.");
                return BadRequest("Imagen requerida.");
            }

            try
            {
                using var image = Image.Load(notice.ImageFile.OpenReadStream());
                if (image.Height > image.Width)
                {
                    _logger.LogWarning("Fin de Create: La imagen para la noticia {NoticeTitle} debe ser horizontal.", notice.Title);
                    return BadRequest("La imagen debe ser horizontal.");
                }

                var imgFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(imgFolder))
                {
                    Directory.CreateDirectory(imgFolder);
                    _logger.LogInformation("Directorio de imágenes '{ImgFolder}' creado.", imgFolder);
                }

                var fileName = Path.GetFileName(notice.ImageFile.FileName);
                var path = Path.Combine(imgFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                    await notice.ImageFile.CopyToAsync(stream);

                notice.Img = $"img/{fileName}";
                await _noticeService.AddNoticeAsync(notice);

                _logger.LogInformation("Fin de Create: Noticia '{NoticeTitle}' (ID: {NoticeId}) creada exitosamente. Imagen: {ImagePath}", notice.Title, notice.Id, notice.Img);
                return CreatedAtAction(nameof(GetById), new { id = notice.Id }, notice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create: Falló al crear noticia con título '{NoticeTitle}'.", notice.Title);
                return StatusCode(500, $"Error al crear noticia: {ex.Message}");
            }
        }





        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] Notice notice)
        {
            _logger.LogInformation("Inicio de Update: Intentando actualizar noticia con ID: {NoticeId}.", id);
            if (id != notice.Id)
            {
                _logger.LogWarning("Fin de Update: ID de noticia en ruta ({RouteId}) no coincide con ID en cuerpo ({BodyId}).", id, notice.Id);
                return BadRequest("ID inválido.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Fin de Update: Fallo de validación del modelo al actualizar noticia {NoticeId}. Errores: {ModelStateErrors}", id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            try
            {
                if (notice.ImageFile != null && notice.ImageFile.Length > 0)
                {
                    using var image = Image.Load(notice.ImageFile.OpenReadStream());
                    if (image.Height > image.Width)
                    {
                        _logger.LogWarning("Fin de Update: La imagen para la noticia {NoticeId} debe ser horizontal.", id);
                        return BadRequest("La imagen debe ser horizontal.");
                    }

                    var fileName = Path.GetFileName(notice.ImageFile.FileName);
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await notice.ImageFile.CopyToAsync(stream);
                    notice.Img = $"img/{fileName}";
                    _logger.LogInformation("Imagen de noticia {NoticeId} actualizada a: {ImagePath}", id, notice.Img);
                }

                await _noticeService.UpdateNoticeAsync(notice);
                _logger.LogInformation("Fin de Update: Noticia con ID {NoticeId} actualizada exitosamente.", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!NoticeExists(notice.Id))
                {
                    _logger.LogWarning("Fin de Update: Fallo de concurrencia. Noticia con ID {NoticeId} no encontrada durante la actualización.", notice.Id);
                    return NotFound();
                }
                _logger.LogError(ex, "Error en Update: Fallo de concurrencia al actualizar noticia con ID {NoticeId}.", id);
                throw; // Re-lanza la excepción si no se maneja aquí.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Update: Falló al actualizar noticia con ID {NoticeId}.", id);
                return StatusCode(500, "Error interno al actualizar.");
            }
        }




        private bool NoticeExists(int id)
        {
            var exists = _context.Notices.Any(n => n.Id == id);
            _logger.LogDebug("Verificando existencia de noticia con ID {NoticeId}: {Exists}", id, exists); // Log de depuración
            return exists;
        }




        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Inicio de Delete: Intentando eliminar noticia con ID: {NoticeId}.", id);
            try
            {
                var notice = await _noticeService.GetNoticeByIdAsync(id);
                if (notice == null)
                {
                    _logger.LogWarning("Fin de Delete: Noticia con ID {NoticeId} no encontrada para eliminación.", id);
                    return NotFound();
                }

                await _noticeService.DeleteNoticeAsync(id);
                _logger.LogInformation("Fin de Delete: Noticia con ID {NoticeId} eliminada correctamente.", id);
                return Ok(new { message = "Noticia eliminada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Delete: Falló al eliminar noticia con ID {NoticeId}.", id);
                return StatusCode(500, "Error interno al eliminar la noticia.");
            }
        }

    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}