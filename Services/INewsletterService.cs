using EnFoco_new.Models;

namespace EnFoco_new.Services
{
    public interface INewsletterService
    {
        Task<List<Newsletter>> GetAllNewsletter();
        Task AddSubscriber(string email);
    }
}