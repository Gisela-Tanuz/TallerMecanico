using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TallerMecanico.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [MaxLength(40)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(40)]
        public string Nombre { get; set; } = string.Empty;

        public int Dni { get; set; }

        [MaxLength(250)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;
    }
}
