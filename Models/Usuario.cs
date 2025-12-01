using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISW2_Primer_parcial.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required]
    [MaxLength(100)]
    public required string NombreUsuario { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [MaxLength(100)]
    public string? NombreCompleto { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoAcceso { get; set; }

    // Foreign Key
    public int IdRol { get; set; }

    // Navegación
    [ForeignKey("IdRol")]
    public Rol? Rol { get; set; }
}
