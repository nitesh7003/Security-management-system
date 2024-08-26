using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class addcomplaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string name { get; set; }  // from alloteflat model

        [Required]
        public int flatno { get; set; }  // from alloteflat model

        [Required]
        [StringLength(500)]
        public string WriteComplaint { get; set; }

        [Required]
        public string complaintstatus { get; set; } = "Pending";  // Default status is "Pending"

        [Required]
        [DataType(DataType.Date)]
        public DateTime raisedate { get; set; } = DateTime.Now;  // Automatically set to current date

        [DataType(DataType.Date)]
        public DateTime? resolvedate { get; set; }  // Set when the admin resolves the complaint
    }
}
