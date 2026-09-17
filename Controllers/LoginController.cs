using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TallerMecanico.Models;

namespace TallerMecanico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IConfiguration _config;

        public LoginController(DataContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] Login login)
        {
            try
            {
                string hashed = Hashear(login.Clave!);

                var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Email == login.Email);
                if (usuario == null || usuario.Clave != hashed)
                    return BadRequest("Email o clave incorrecta");

                var key = new SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(_config["TokenAuthentication:SecretKey"]!));
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var rol = string.IsNullOrWhiteSpace(usuario.Rol) ? "Empleado" : usuario.Rol;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim("FullName", usuario.Nombre),
                    new Claim(ClaimTypes.Role, rol),
                };

                var token = new JwtSecurityToken(
                    issuer: _config["TokenAuthentication:Issuer"],
                    audience: _config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(360),
                    signingCredentials: credenciales
                );

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("registrar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Registrar([FromForm] string email, [FromForm] string clave, [FromForm] string nombre, [FromForm] string? rol)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(nombre))
                return BadRequest("Debe completar email, clave y nombre");

            try
            {
                var existe = await _db.Usuarios.AnyAsync(u => u.Email == email);
                if (existe)
                    return BadRequest("Ya existe un usuario con ese email");

                var usuario = new Usuario
                {
                    Email = email,
                    Nombre = nombre,
                    Clave = Hashear(clave),
                    Rol = string.IsNullOrWhiteSpace(rol) ? "Empleado" : rol
                };

                _db.Usuarios.Add(usuario);
                await _db.SaveChangesAsync();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("cambiarPass")]
        [Authorize]
        public async Task<IActionResult> CambiarPass([FromForm] string clVieja, [FromForm] string clNueva)
        {
            string emailUsuario = User.Identity!.Name!;
            if (emailUsuario == null)
                return Unauthorized("Usuario no autorizado");

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Email == emailUsuario);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            try
            {
                string hashed = Hashear(clVieja);
                if (usuario.Clave != hashed)
                    return BadRequest("No fue posible actualizar la contraseÃ±a");

                usuario.Clave = Hashear(clNueva);
                _db.Usuarios.Update(usuario);
                await _db.SaveChangesAsync();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Usuario>>> ListarUsuarios()
        {
            try
            {
                return await _db.Usuarios.ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetUsuarios()
        {
            try
            {
                var email = User.Identity!.Name;
                return await _db.Usuarios.SingleOrDefaultAsync(x => x.Email == email)
                    ?? (ActionResult<Usuario>)NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string Hashear(string clave)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: clave,
                salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"]!),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
        }
    }
}
