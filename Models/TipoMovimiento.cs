using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class TipoMovimiento
{
    [Key]
    public int IdTipoMovimiento { get; set; }

    [Required]
    public required string Tipo { get; set; } // Entrada or Salida
}