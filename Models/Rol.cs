using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class Rol
{
    [Key]
    public int IdRol { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Nombre { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
