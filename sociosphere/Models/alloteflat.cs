using System.ComponentModel.DataAnnotations;

namespace sociosphere.Models
{
    public class alloteflat
    {


        public int Id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public int flatno { get; set; }

        [Required]
        public string floorno { get; set; }

        [Required]
        public string wingname { get; set; }

        [Required]
        public string flattype { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime moveindate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime allotdate { get; set; } 
    }
}
