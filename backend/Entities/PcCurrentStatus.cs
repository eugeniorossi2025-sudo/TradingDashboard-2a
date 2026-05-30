using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Live PC/bot status (production legacy: dbo.Pc_CurrentStatus).
/// </summary>
[Table("Pc_CurrentStatus")]
public class PcCurrentStatus
{
    [Key]
    [Column("COMPUTER")]
    [MaxLength(50)]
    public string Computer { get; set; } = string.Empty;

    [Column("KEY_ULTIMO")]
    public decimal KeyUltimo { get; set; }

    [Column("DT_ULTIMO")]
    public DateTime DtUltimo { get; set; }

    [Column("ACCOUNT")]
    [MaxLength(500)]
    public string? Account { get; set; }

    [Column("TAVOLO")]
    [MaxLength(500)]
    public string? Tavolo { get; set; }

    [Column("SALDO_INIZIALE")]
    public decimal SaldoIniziale { get; set; }

    [Column("SALDO_ISTANTANEO")]
    public decimal SaldoIstantaneo { get; set; }

    [Column("MARGINE")]
    public decimal Margine { get; set; }

    [Column("MEDIA_ORA")]
    public decimal MediaOra { get; set; }

    [Column("VALORE_GIOCATO")]
    public decimal ValoreGiocato { get; set; }

    [Column("COLPO_MARTINGALA")]
    public int ColpoMartingala { get; set; }

    [Column("STATO")]
    [MaxLength(100)]
    public string? Stato { get; set; }

    [Column("COLORE")]
    [MaxLength(20)]
    public string? Colore { get; set; }

    [Column("CHOSEN_COLOR")]
    [MaxLength(1)]
    public string? ChosenColor { get; set; }

    [Column("MAZZO")]
    [MaxLength(50)]
    public string? Mazzo { get; set; }

    [Column("PBT")]
    [MaxLength(1)]
    public string? Pbt { get; set; }

    [Column("ORE")]
    public decimal Ore { get; set; }

    [Column("LAST_UPDATE")]
    public DateTime LastUpdate { get; set; }

    [Column("LAST_ADVICE")]
    [MaxLength(4000)]
    public string? LastAdvice { get; set; }

    [Column("LAST_INFO")]
    [MaxLength(4000)]
    public string? LastInfo { get; set; }

    [Column("VALUTAZIONE_RISULTATO")]
    [MaxLength(4000)]
    public string? ValutazioneRisultato { get; set; }
}
