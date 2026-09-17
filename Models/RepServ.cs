using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TallerMecanico.Models
{
    [Table("rep_serv")]
    public class RepServ
    {
        [Key]
        public int IdRepServ { get; set; }

        public int IdRespuesto { get; set; }

        public int IdServicio { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio { get; set; }

        [ForeignKey("IdRespuesto")]
        public Repuesto? Repuesto { get; set; }
    }
}
