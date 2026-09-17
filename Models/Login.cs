using System.ComponentModel.DataAnnotations;
namespace TallerMecanico.Models;

   public class Login
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Clave { get; set; }
    }