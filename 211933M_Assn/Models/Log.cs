using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace _211933M_Assn.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Action { get; set; } = string.Empty;
        [Required]
        public DateTime Date_Created { get; set; } = DateTime.Now;

        public User? LogUser { get; set; }
    }
}
