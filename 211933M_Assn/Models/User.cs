using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace _211933M_Assn.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string CCno { get; set; } = string.Empty;
        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(6|8|9)\d{7}$", ErrorMessage = "Invalid Phone number.")]
        [DataType(DataType.PhoneNumber)]
        public int Phone { get; set; }

        [Required, MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        [DataType(DataType.ImageUrl)]
        public string? ImageURL { get; set; } = string.Empty;
        [Required]
        public string Aboutme { get; set; } = string.Empty;
    }
}
