using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class userreg
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string photo { get; set; }


        [Required]
        [StringLength(100)]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [StringLength(100)]
        public string city { get; set; }

        [Required]
        [StringLength(50)]
        public string role { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }

    }
}


    
