using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerMecanico.Models;

namespace TallerMecanico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly DataContext _db;

        public ClientesController(DataContext db)
        {
            _db = db;
        }

        [HttpGet("listarClientes")]
        public async Task<ActionResult<IEnumerable<Cliente>>> ListarClientes()
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                return Ok(await _db.Clientes.ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("perfilCliente/{id}")]
        public async Task<ActionResult<Cliente>> PerfilCliente(int id)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var cliente = await _db.Clientes.FindAsync(id);
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("editarClientes")]
        public async Task<IActionResult> EditarClientes([FromBody] Cliente cliente)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                var existente = await _db.Clientes.FindAsync(cliente.IdCliente);
                if (existente == null)
                    return NotFound("Cliente no encontrado");

                if (!string.IsNullOrEmpty(cliente.Nombre))
                    existente.Nombre = cliente.Nombre;
                if (!string.IsNullOrEmpty(cliente.Apellido))
                    existente.Apellido = cliente.Apellido;
                if (cliente.Dni != 0)
                    existente.Dni = cliente.Dni;
                if (!string.IsNullOrEmpty(cliente.Email))
                    existente.Email = cliente.Email;
                if (!string.IsNullOrEmpty(cliente.Telefono))
                    existente.Telefono = cliente.Telefono;

                _db.Clientes.Update(existente);
                await _db.SaveChangesAsync();

                return Ok(existente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("agregarClientes")]
        public async Task<IActionResult> AgregarClientes([FromBody] Cliente cliente)
        {
            string? usuario = User.Identity?.Name;
            if (usuario == null)
            {
                return Unauthorized("Usuario no autorizado");
            }

            try
            {
                _db.Clientes.Add(cliente);
                await _db.SaveChangesAsync();

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
