using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class addflat
    {


        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string flatno { get; set; }

        [Required]
        [Range(1, 100)]
        public int floorno { get; set; }

        [Required]
        [StringLength(10)]
        public string wingname { get; set; }

        [Required]
        [StringLength(10)]
        public string flattype { get; set; } // 1 BHK, 2 BHK, 3 BHK
    }
}
