using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISW2_Primer_parcial.Models;

public class MovimientosInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    [Required]
    public int IdProductoAsociado { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    public int IdTipoMovimiento { get; set; }

    public DateTime UltimaFechaActualizacion { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey("IdProductoAsociado")]
    public Producto? Producto { get; set; }
    public TipoMovimiento? TipoMovimiento { get; set; }
}