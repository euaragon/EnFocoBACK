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
            var notices = await _noticeService.GetAllNoticesAsync();
            return Ok(notices);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notice = await _noticeService.GetNoticeByIdAsync(id);
            return notice == null ? NotFound() : Ok(notice);
        }

        [HttpGet("section/{section}")]
        public IActionResult GetBySection(string section)
        {
            if (!Enum.TryParse(section, out NoticeSection parsedSection))
                return BadRequest("Sección inválida.");

            var notices = _noticeService.GetNotice()
                .Where(n => n.Section == parsedSection)
                .OrderByDescending(n => n.Id)
                .ToList();

            return Ok(notices);
        }

        [HttpGet("search")]
        public IActionResult Search(string searchTerm, int page = 1, int pageSize = 6)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Término de búsqueda requerido.");

            var query = _noticeService.SearchNotices(searchTerm).AsQueryable();
            var totalCount = query.Count();
            var notices = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagedResult<Notice>
            {
                Data = notices,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Notice notice)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (notice.ImageFile == null || notice.ImageFile.Length == 0)
                return BadRequest("Imagen requerida.");

            try
            {
                using var image = Image.Load(notice.ImageFile.OpenReadStream());
                if (image.Height > image.Width)
                    return BadRequest("La imagen debe ser horizontal.");

                var imgFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(imgFolder))
                {
                    Directory.CreateDirectory(imgFolder);
                }

                var fileName = Path.GetFileName(notice.ImageFile.FileName);
                var path = Path.Combine(imgFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                    await notice.ImageFile.CopyToAsync(stream);

                notice.Img = $"img/{fileName}";
                await _noticeService.AddNoticeAsync(notice);

                return CreatedAtAction(nameof(GetById), new { id = notice.Id }, notice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear noticia");
                return StatusCode(500, $"Error al crear noticia: {ex.Message}");
            }
        }



      

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] Notice notice)
        {
            if (id != notice.Id)
                return BadRequest("ID inválido.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (notice.ImageFile != null && notice.ImageFile.Length > 0)
                {
                    using var image = Image.Load(notice.ImageFile.OpenReadStream());
                    if (image.Height > image.Width)
                        return BadRequest("La imagen debe ser horizontal.");

                    var fileName = Path.GetFileName(notice.ImageFile.FileName);
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await notice.ImageFile.CopyToAsync(stream);
                    notice.Img = $"img/{fileName}";
                }

                await _noticeService.UpdateNoticeAsync(notice);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoticeExists(notice.Id))
                    return NotFound();

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar noticia {id}.");
                return StatusCode(500, "Error interno.");
            }
        }





        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var notice = await _noticeService.GetNoticeByIdAsync(id);
            if (notice == null)
                return NotFound();

            await _noticeService.DeleteNoticeAsync(id);
            return Ok(new { message = "Noticia eliminada correctamente." });
        }

        private bool NoticeExists(int id) => _context.Notices.Any(n => n.Id == id);
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
