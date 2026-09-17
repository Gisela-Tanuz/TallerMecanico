using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TallerMecanico.Models
{
    [Table("Repuestos")]
    public class Repuesto
    {
        [Key]
        public int IdRepuesto { get; set; }

        [MaxLength(250)]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Imagen { get; set; }

        [MaxLength(250)]
        public string Descripcion { get; set; } = string.Empty;

        public double Precio { get; set; }

        public int Cantidad { get; set; }
    }
}
