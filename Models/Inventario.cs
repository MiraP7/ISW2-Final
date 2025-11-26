using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class Inventario
{
    [Key]
    public int IdProducto { get; set; }

    [Required]
    public int Existencia { get; set; }

    public DateTime UltimaFechaActualizacion { get; set; } = DateTime.Now;

    // Navigation property
    public Producto? Producto { get; set; }
}