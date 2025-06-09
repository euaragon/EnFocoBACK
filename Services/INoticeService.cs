using EnFoco_new.Models;

namespace EnFoco_new.Services
{
    public interface INoticeService
    {
        Task<List<Notice>> GetAllNoticesAsync();
        IList<Notice> GetNotice();
        Task<Notice?> GetNoticeByIdAsync(int id);
        Task AddNoticeAsync(Notice notice);
        Task DeleteNoticeAsync(int id);
        Task UpdateNoticeAsync(Notice notice);
        IList<Notice> SearchNotices(string searchTerm);
        Task<IList<Notice>> GetPaginatedNoticesAsync(int page, int pageSize, string? searchTerm = null, string? category = null);
        Task<int> GetTotalNoticesCountAsync(string? searchTerm = null, string? category = null);
    }
}
