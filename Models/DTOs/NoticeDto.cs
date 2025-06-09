namespace EnFoco_new.DTOs
{
    public class NoticeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}