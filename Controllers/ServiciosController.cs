using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly DataContext _db;

        public ServiciosController(DataContext db)
        {
            _db = db;
        }

        [HttpGet("listarServicios")]
        public async Task<ActionResult<IEnumerable<Servicio>>> ListarServicios()
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                return Ok(await _db.Servicios
                    .Include(s => s.cliente)
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("misServicios")]
        public async Task<ActionResult<IEnumerable<Servicio>>> MisServicios()
        {
            string? email = User.Identity?.Name;
            if (email == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
                if (usuario == null)
                    return NotFound("Usuario no encontrado");

                var servicios = await _db.Servicios
                    .Where(s => s.IdUsuario == usuario.IdUsuario)
                    .Include(s => s.cliente)
                    .ToListAsync();

                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("obtenerRepuestosPorServicio/{id}")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerRepuestosPorServicio(int id)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var existeServicio = await _db.Servicios.AnyAsync(s => s.IdServicios == id);
                if (!existeServicio)
                    return NotFound("Servicio no encontrado");

                var repuestos = await _db.RepServs
                    .AsNoTracking()
                    .Where(rs => rs.IdServicio == id)
                    .Select(rs => new
                    {
                        rs.IdRepServ,
                        rs.IdRespuesto,
                        rs.Repuesto!.Codigo,
                        rs.Repuesto!.Descripcion,
                        rs.Cantidad,
                        rs.Precio
                    })
                    .ToListAsync();

                return Ok(repuestos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("agregarServicios")]
        public async Task<IActionResult> AgregarServicios([FromBody] AgregarServicioRequest request)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                
                //solo se usan lso ID de usuario y clientes para evitar que se insertar usuario y clientes que ya existen
                request.Servicio.Usuario = null;
                request.Servicio.cliente = null;

                _db.Servicios.Add(request.Servicio);
                await _db.SaveChangesAsync();

                if (request.Repuestos != null)
                {
                    foreach (var r in request.Repuestos)
                    {
                        var (ok, error, _, _) = await RepServHelper.UsarRepuestoAsync(
                            _db, r.IdRespuesto, request.Servicio.IdServicios, r.Cantidad, r.Precio);

                        if (!ok)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(error);
                        }
                    }

                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(request.Servicio);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("modificarServicio")]
        public async Task<IActionResult> ModificarServicio([FromBody] Servicio servicio)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var existente = await _db.Servicios.FindAsync(servicio.IdServicios);
                if (existente == null)
                    return NotFound("Servicio no encontrado");

                if (servicio.IdUsuario != 0)
                    existente.IdUsuario = servicio.IdUsuario;
                if (servicio.IdCliente != 0)
                    existente.IdCliente = servicio.IdCliente;
                if (!string.IsNullOrEmpty(servicio.Km))
                    existente.Km = servicio.Km;
                if (!string.IsNullOrEmpty(servicio.Marca))
                    existente.Marca = servicio.Marca;
                if (!string.IsNullOrEmpty(servicio.Modelo))
                    existente.Modelo = servicio.Modelo;
                if (!string.IsNullOrEmpty(servicio.Patente))
                    existente.Patente = servicio.Patente;
                if (!string.IsNullOrEmpty(servicio.TrabajoARealizar))
                    existente.TrabajoARealizar = servicio.TrabajoARealizar;
                if (servicio.FechaIngreso != default)
                    existente.FechaIngreso = servicio.FechaIngreso;
                if (servicio.FechaEgreso != default)
                    existente.FechaEgreso = servicio.FechaEgreso;
                if (servicio.FechaProxServ != null)
                    existente.FechaProxServ = servicio.FechaProxServ;
                if (servicio.Total != null)
                    existente.Total = servicio.Total;

                _db.Servicios.Update(existente);
                await _db.SaveChangesAsync();

                return Ok(existente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("eliminarServicios/{id}")]
        public async Task<IActionResult> EliminarServicios(int id)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var existente = await _db.Servicios.FindAsync(id);
                if (existente == null)
                    return NotFound("Servicio no encontrado");

                // Repuestos asociados a este servicio en rep_serv.
                var repServs = await _db.RepServs
                    .Where(rs => rs.IdServicio == id)
                    .ToListAsync();

                // Devuelve el stock de cada repuesto usado y borra el registro.
                foreach (var rs in repServs)
                {
                    var repuesto = await _db.Repuestos.FindAsync(rs.IdRespuesto);
                    if (repuesto != null)
                        repuesto.Cantidad += rs.Cantidad;
                }
                _db.RepServs.RemoveRange(repServs);

                // Primero se borran los hijos (rep_serv) para respetar la FK.
                await _db.SaveChangesAsync();

                // Luego el servicio (padre).
                _db.Servicios.Remove(existente);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(existente);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
