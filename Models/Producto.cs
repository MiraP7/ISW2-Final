using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required]
    public required string Nombre { get; set; }

    // CodigoProducto se genera automáticamente, no debe enviarse en POST/PUT
    public string CodigoProducto { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Required]
    public decimal PrecioVenta { get; set; }

    [Required]
    public int MinimoExistencia { get; set; } = 0;

    // Propiedades de sistema - no modificables por el usuario
    public bool Eliminado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UltimaFechaActualizacion { get; set; } = DateTime.UtcNow;
}