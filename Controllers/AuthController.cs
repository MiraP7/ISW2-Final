using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;
using ISW2_Primer_parcial.DTOs;
using ISW2_Primer_parcial.Services;
using ISW2_Primer_parcial.Attributes;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("registro")]
    [PermitirAnonimo]
    public async Task<ActionResult<AuthResponseDTO>> Registro(RegistroUsuarioDTO dto)
    {
        try
        {
            // Verificar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { mensaje = "Ya existe un usuario con este email" });
            }

            // Verificar si el nombre de usuario ya existe
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            {
                return BadRequest(new { mensaje = "Ya existe un usuario con este nombre de usuario" });
            }

            // Obtener el rol por defecto (Usuario = 2)
            var rolUsuario = await _context.Roles.FindAsync(2);
            if (rolUsuario == null)
            {
                return StatusCode(500, new { mensaje = "Error de configuración: Rol por defecto no encontrado" });
            }

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                NombreCompleto = dto.NombreCompleto,
                IdRol = 2, // Rol Usuario por defecto
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Cargar el rol
            usuario.Rol = rolUsuario;

            var token = _jwtService.GenerarToken(usuario);

            return CreatedAtAction(nameof(ObtenerPerfil), new
            {
                idUsuario = usuario.IdUsuario,
                nombreUsuario = usuario.NombreUsuario,
                email = usuario.Email,
                nombreCompleto = usuario.NombreCompleto,
                rol = rolUsuario.Nombre,
                token = token,
                expiracion = DateTime.UtcNow.AddHours(24),
                mensaje = "Usuario registrado exitosamente"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al registrar usuario", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    [PermitirAnonimo]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto)
    {
        try
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            if (!usuario.Activo)
            {
                return Unauthorized(new { mensaje = "Usuario desactivado. Contacte al administrador." });
            }

            if (!PasswordHelper.VerifyPassword(dto.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new AuthResponseDTO
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                Rol = usuario.Rol?.Nombre ?? "Usuario",
                Token = token,
                Expiracion = DateTime.UtcNow.AddHours(24)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al iniciar sesión", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Obtener perfil del usuario autenticado
    /// </summary>
    [HttpGet("perfil")]
    [RequiereAutenticacion]
    public async Task<ActionResult<UsuarioResponseDTO>> ObtenerPerfil()
    {
        var userId = HttpContext.Items["IdUsuario"] as int?;
        if (userId == null)
        {
            return Unauthorized(new { mensaje = "No autorizado" });
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId);

        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }

        return Ok(new UsuarioResponseDTO
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Activo = usuario.Activo,
            Rol = usuario.Rol?.Nombre ?? "Usuario",
            FechaCreacion = usuario.FechaCreacion,
            UltimoAcceso = usuario.UltimoAcceso
        });
    }

    /// <summary>
    /// Cambiar contraseña del usuario autenticado
    /// </summary>
    [HttpPut("cambiar-password")]
    [RequiereAutenticacion]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDTO dto)
    {
        var userId = HttpContext.Items["IdUsuario"] as int?;
        if (userId == null)
        {
            return Unauthorized(new { mensaje = "No autorizado" });
        }

        var usuario = await _context.Usuarios.FindAsync(userId);
        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }

        if (!PasswordHelper.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
        {
            return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });
        }

        usuario.PasswordHash = PasswordHelper.HashPassword(dto.NuevaPassword);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada exitosamente" });
    }

    // ======= ENDPOINTS SOLO PARA ADMINISTRADORES =======

    /// <summary>
    /// Obtener todos los usuarios (Solo Admin)
    /// </summary>
    [HttpGet("usuarios")]
    [RequiereAdmin]
    public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>> ObtenerUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Select(u => new UsuarioResponseDTO
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                Email = u.Email,
                NombreCompleto = u.NombreCompleto,
                Activo = u.Activo,
                Rol = u.Rol != null ? u.Rol.Nombre : "Usuario",
                FechaCreacion = u.FechaCreacion,
                UltimoAcceso = u.UltimoAcceso
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    /// <summary>
    /// Obtener todos los roles (Solo Admin)
    /// </summary>
    [HttpGet("roles")]
    [RequiereAdmin]
    public async Task<ActionResult<IEnumerable<RolResponseDTO>>> ObtenerRoles()
    {
        var roles = await _context.Roles
            .Select(r => new RolResponseDTO
            {
                IdRol = r.IdRol,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CantidadUsuarios = r.Usuarios.Count
            })
            .ToListAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Crear un nuevo rol (Solo Admin)
    /// </summary>
    [HttpPost("roles")]
    [RequiereAdmin]
    public async Task<ActionResult<RolResponseDTO>> CrearRol(CrearRolDTO dto)
    {
        try
        {
            // Verificar si ya existe un rol con ese nombre
            if (await _context.Roles.AnyAsync(r => r.Nombre == dto.Nombre))
            {
                return BadRequest(new { mensaje = "Ya existe un rol con ese nombre" });
            }

            var rol = new Rol
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerRoles), new RolResponseDTO
            {
                IdRol = rol.IdRol,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                CantidadUsuarios = 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al crear rol", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Asignar rol a un usuario (Solo Admin)
    /// </summary>
    [HttpPut("asignar-rol")]
    [RequiereAdmin]
    public async Task<IActionResult> AsignarRol(AsignarRolDTO dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }

            var rol = await _context.Roles.FindAsync(dto.IdRol);
            if (rol == null)
            {
                return NotFound(new { mensaje = "Rol no encontrado" });
            }

            usuario.IdRol = dto.IdRol;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Rol '{rol.Nombre}' asignado al usuario '{usuario.NombreUsuario}' exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al asignar rol", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Activar/Desactivar usuario (Solo Admin)
    /// </summary>
    [HttpPut("usuarios/{id}/estado")]
    [RequiereAdmin]
    public async Task<IActionResult> CambiarEstadoUsuario(int id, [FromQuery] bool activo)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }

        // Evitar que un admin se desactive a sí mismo
        var currentUserId = HttpContext.Items["IdUsuario"] as int?;
        if (id == currentUserId && !activo)
        {
            return BadRequest(new { mensaje = "No puedes desactivar tu propia cuenta" });
        }

        usuario.Activo = activo;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = activo ? "Usuario activado" : "Usuario desactivado" });
    }

    /// <summary>
    /// Crear usuario con rol específico (Solo Admin)
    /// </summary>
    [HttpPost("usuarios")]
    [RequiereAdmin]
    public async Task<ActionResult<UsuarioResponseDTO>> CrearUsuarioConRol(
        [FromBody] RegistroUsuarioDTO dto,
        [FromQuery] int? idRol = null)
    {
        try
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { mensaje = "Ya existe un usuario con este email" });
            }

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            {
                return BadRequest(new { mensaje = "Ya existe un usuario con este nombre de usuario" });
            }

            var rolId = idRol ?? 2; // Por defecto Usuario
            var rol = await _context.Roles.FindAsync(rolId);
            if (rol == null)
            {
                return BadRequest(new { mensaje = "Rol no encontrado" });
            }

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                NombreCompleto = dto.NombreCompleto,
                IdRol = rolId,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerUsuarios), new UsuarioResponseDTO
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                Activo = usuario.Activo,
                Rol = rol.Nombre,
                FechaCreacion = usuario.FechaCreacion,
                UltimoAcceso = usuario.UltimoAcceso
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al crear usuario", detalle = ex.Message });
        }
    }
}
