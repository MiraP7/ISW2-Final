using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.DTOs;

// DTO para registro de usuario
public class RegistroUsuarioDTO
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
    [MaxLength(100)]
    public required string NombreUsuario { get; set; }

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(100)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public required string Password { get; set; }

    [MaxLength(100)]
    public string? NombreCompleto { get; set; }
}

// DTO para login
public class LoginDTO
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida")]
    public required string Password { get; set; }
}

// DTO para respuesta de autenticación
public class AuthResponseDTO
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
}

// DTO para asignar rol
public class AsignarRolDTO
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El ID del rol es requerido")]
    public int IdRol { get; set; }
}

// DTO para crear rol
public class CrearRolDTO
{
    [Required(ErrorMessage = "El nombre del rol es requerido")]
    [MinLength(2, ErrorMessage = "El nombre del rol debe tener al menos 2 caracteres")]
    [MaxLength(50)]
    public required string Nombre { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }
}

// DTO para respuesta de usuario
public class UsuarioResponseDTO
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public bool Activo { get; set; }
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

// DTO para respuesta de rol
public class RolResponseDTO
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadUsuarios { get; set; }
}

// DTO para cambiar contraseña
public class CambiarPasswordDTO
{
    [Required(ErrorMessage = "La contraseña actual es requerida")]
    public required string PasswordActual { get; set; }

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
    public required string NuevaPassword { get; set; }
}
