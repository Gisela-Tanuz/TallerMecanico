using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TallerMecanico.Models
{
    [Table("Servicios")]
    public class Servicio
    {
        [Key]
        public int IdServicios { get; set; }

        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        [JsonIgnore]
        public Usuario? Usuario {get; set;}

        public int IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        [JsonIgnore]
        public Cliente? cliente {get; set; }

        [MaxLength(100)]
        public string? Km { get; set; }

        [MaxLength(60)]
        public string? Marca { get; set; }

        [MaxLength(60)]
        public string? Modelo { get; set; }

        [MaxLength(15)]
        public string? Patente { get; set; }

        [MaxLength(500)]
        public string? TrabajoARealizar { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime FechaEgreso { get; set; }

        public DateTime? FechaProxServ { get; set; }
        
        public double? Total { get; set; }
    }
}
