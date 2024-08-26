using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Announcement { get; set; }

        [Required]
        public DateTime AnnounceDate { get; set; }
    }
}
