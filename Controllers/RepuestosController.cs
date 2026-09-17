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
    public class RepuestosController : ControllerBase
    {
        private readonly DataContext _db;
        private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RepuestosController(DataContext db)
        {
            _db = db;
        }

        [HttpGet("listarRepuestos")]
        public async Task<ActionResult<IEnumerable<Repuesto>>> ListarRepuestos()
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                return Ok(await _db.Repuestos.ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerRepuestoPorId/{id}")]
        public async Task<ActionResult<Repuesto>> ObtenerRepuestoPorId(int id)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var repuesto = await _db.Repuestos.FindAsync(id);
                if (repuesto == null)
                    return NotFound("Repuesto no encontrado");

                return Ok(repuesto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("editarRepuestos")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditarRepuestos([FromForm] string repuesto, IFormFile? imagen)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var datos = System.Text.Json.JsonSerializer.Deserialize<Repuesto>(repuesto, _jsonOptions);
                if (datos == null)
                    return BadRequest("Datos de repuesto invÃ¡lidos");

                var existente = await _db.Repuestos.FindAsync(datos.IdRepuesto);
                if (existente == null)
                    return NotFound("Repuesto no encontrado");

                if (!string.IsNullOrEmpty(datos.Codigo))
                    existente.Codigo = datos.Codigo;
                if (!string.IsNullOrEmpty(datos.Descripcion))
                    existente.Descripcion = datos.Descripcion;
                if (datos.Precio != 0)
                    existente.Precio = datos.Precio;
                if (datos.Cantidad != 0)
                    existente.Cantidad = datos.Cantidad;

                if (imagen != null && imagen.Length > 0) //verifico que la foto no este vacia
                {
                    string pathUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadFile"); //construyo ruta
                    if (!Directory.Exists(pathUpload)) //si la carpeta no existe la creo
                    {
                        Directory.CreateDirectory(pathUpload);
                    }

                    string nameFile = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName); //nombre para el archivo
                    string pathCompleto = Path.Combine(pathUpload, nameFile); //ruta completa del archivo

                    using (var stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    existente.Imagen = nameFile; //asigno el nombre del archivo al repuesto
                }

                _db.Repuestos.Update(existente);
                await _db.SaveChangesAsync();

                return Ok(existente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("agregarRepuestos")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AgregarRepuestos([FromForm] string repuesto, IFormFile? imagen)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var nuevoRepuesto = System.Text.Json.JsonSerializer.Deserialize<Repuesto>(repuesto, _jsonOptions);
                if (nuevoRepuesto == null)
                    return BadRequest("Datos de repuesto invÃ¡lidos");

                if (imagen != null && imagen.Length > 0) //verifico que la foto no este vacia
                {
                    string pathUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadFile"); //construyo ruta
                    if (!Directory.Exists(pathUpload)) //si la carpeta no existe la creo
                    {
                        Directory.CreateDirectory(pathUpload);
                    }

                    string nameFile = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName); //nombre para el archivo
                    string pathCompleto = Path.Combine(pathUpload, nameFile); //ruta completa del archivo

                    using (var stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    nuevoRepuesto.Imagen = nameFile; //asigno el nombre del archivo al repuesto
                }

                _db.Repuestos.Add(nuevoRepuesto);
                await _db.SaveChangesAsync();

                return Ok(nuevoRepuesto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("usarRepuesto")]
        public async Task<IActionResult> UsarRepuesto([FromBody] RepServ uso)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var servicio = await _db.Servicios.FindAsync(uso.IdServicio);
                if (servicio == null)
                    return NotFound("Servicio no encontrado");

                var (ok, error, repuesto, registro) = await RepServHelper.UsarRepuestoAsync(
                    _db, uso.IdRespuesto, uso.IdServicio, uso.Cantidad, uso.Precio);

                if (!ok)
                    return BadRequest(error);

                await _db.SaveChangesAsync();

                return Ok(new { repuesto, registro });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerRepuestosPorServicio/{idServicio}")]
        public async Task<IActionResult> ObtenerRepuestosPorServicio(int idServicio)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
                return Unauthorized("Usuario no autorizado");

            try
            {
                var resultado = await _db.RepServs
                    .Where(rs => rs.IdServicio == idServicio)
                    .Include(rs => rs.Repuesto)
                    .ToListAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("eliminarRepuestos/{id}")]
        public async Task<IActionResult> EliminarRepuestos(int id)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var existente = await _db.Repuestos.FindAsync(id);
                if (existente == null)
                    return NotFound("Repuesto no encontrado");

                _db.Repuestos.Remove(existente);
                await _db.SaveChangesAsync();

                return Ok(existente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
