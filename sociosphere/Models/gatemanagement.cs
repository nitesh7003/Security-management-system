using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class gatemanagement
    {
        public int Id { get; set; }

        [Required]
        public string WingName { get; set; }

        [Required]
        public int FlatNo { get; set; }

        [Required]
        public string VisitorName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Name { get; set; } // Person to meet

        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime InDateTime { get; set; } = DateTime.Now;

        public DateTime? OutDateTime { get; set; }

        [Required]
        public string Status { get; set; } = "In";

        public string Action { get; set; } // To be managed by GateMan
    }
}
