using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class billmanagement
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string WingName { get; set; }

        [Required]
        public int FlatNo { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal AmountPay { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Month { get; set; }

        [Required]
        public string PaidStatus { get; set; } = "Pending"; // Default status is "Pending"

        [DataType(DataType.Date)]
        public DateTime BillReleaseDt { get; set; } // Current system date when admin adds the bill

        [DataType(DataType.Date)]
        public DateTime? BillSbmtDate { get; set; } // Date when the user pays the bill
    }
}
