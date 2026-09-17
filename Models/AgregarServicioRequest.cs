namespace TallerMecanico.Models
{
    public class AgregarServicioRequest
    {
        public Servicio Servicio { get; set; } = new();

        public List<RepuestoUsoRequest>? Repuestos { get; set; }
    }

    public class RepuestoUsoRequest
    {
        public int IdRespuesto { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio { get; set; }
    }
}
