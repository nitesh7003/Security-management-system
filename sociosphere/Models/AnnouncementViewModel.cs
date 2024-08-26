using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class AnnouncementViewModel
    {
        [Required]
        [StringLength(500)]
        public string Announcement { get; set; }
    }
}
