using TallerMecanico.Models;

namespace TallerMecanico.Services
{
    public static class RepServHelper
    {
        public static async Task<(bool Ok, string? Error, Repuesto? Repuesto, RepServ? Registro)> UsarRepuestoAsync(
            DataContext _db, int idRepuesto, int idServicio, int cantidad, decimal precio)
        {
            if (cantidad <= 0)
                return (false, "La cantidad debe ser mayor a 0", null, null);

            if (precio <= 0)
                return (false, "El precio debe ser mayor a 0", null, null);

            var repuesto = await _db.Repuestos.FindAsync(idRepuesto);
            if (repuesto == null)
                return (false, "Repuesto no encontrado", null, null);

            if (repuesto.Cantidad < cantidad)
                return (false, $"Stock insuficiente para el repuesto '{repuesto.Descripcion}'", null, null);

            repuesto.Cantidad -= cantidad;
            _db.Repuestos.Update(repuesto);

            var registro = new RepServ
            {
                IdRespuesto = idRepuesto,
                IdServicio = idServicio,
                Cantidad = cantidad,
                Precio = precio
            };
            _db.RepServs.Add(registro);

            return (true, null, repuesto, registro);
        }
    }
}
