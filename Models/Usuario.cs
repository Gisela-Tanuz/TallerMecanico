using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TallerMecanico.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        // "Administrador" o "Empleado"
        public string Rol { get; set; } = "Empleado";
    }
}
