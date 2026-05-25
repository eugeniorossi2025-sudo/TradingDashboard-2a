using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

/// <summary>
/// Payload for collaudo mirror upsert (matches bot update-params fields).
/// </summary>
public class MirrorPcStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Computer { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Account { get; set; }

    [MaxLength(500)]
    public string? Tavolo { get; set; }

    public decimal SaldoIniziale { get; set; }

    public decimal SaldoIstantaneo { get; set; }

    public decimal Margine { get; set; }

    public decimal ValoreGiocato { get; set; }

    public int ColpoMartingala { get; set; }

    [MaxLength(100)]
    public string? Stato { get; set; }

    [MaxLength(50)]
    public string? Mazzo { get; set; }

    [MaxLength(1)]
    public string? Pbt { get; set; }

    public decimal Ore { get; set; }

    [MaxLength(20)]
    public string? Colore { get; set; }
}
