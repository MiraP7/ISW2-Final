using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class ApiKey
{
    [Key]
    public int IdApiKey { get; set; }

    [Required]
    public required string Clave { get; set; }

    [Required]
    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaVencimiento { get; set; }

    public DateTime UltimaFechaUso { get; set; } = DateTime.UtcNow;
}
