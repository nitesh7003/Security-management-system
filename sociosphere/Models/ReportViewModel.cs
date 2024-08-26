using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class ReportViewModel
    {
        [Required]
        public string ReportFor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
