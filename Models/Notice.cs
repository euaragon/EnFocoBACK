﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnFoco_new.Models
{

    public enum NoticeCategory
    {
        [Display(Name = "Tribunal de Cuentas")]
        tribunal,
        [Display(Name = "Fiscalía de Estado")]
        fiscalia,
        [Display(Name = "Ética Pública Mendoza")]
        etica
    }

    public enum NoticeSection
    {
        category1,
        category2,
        category3
    }

    public class Notice
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Subtitle { get; set; } 
        [Required]
        public string? Issue { get; set; } 
        [Required]
        public string? Text { get; set; } 
        
        public string? Img { get; set; } 
        public bool IsFeatured { get; set; } = false;
        public NoticeCategory Category { get; set; }
        public NoticeSection Section { get; set; }
        public string? ImageUrl { get; internal set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }

    

}
