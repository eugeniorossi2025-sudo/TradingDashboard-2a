namespace Decisore.Models
{
    public class RequestParams
    {
        public string? SALDO_INIZIALE { get; set; } = "0";
        public string? VALORE_GIOCATO { get; set; } = "0";
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public string COMPUTER { get; set; }
        public string? TAVOLO { get; set; }
        public string? MARGINE { get; set; } = "0";
        public string? COLPO_MARTINGALA { get; set; } = "0";
        public string? PBT { get; set; }
        public string? MAZZO { get; set; }
        public string? TEMPO { get; set; }
        public string? SALDO_ISTANTANEO { get; set; } = "0";
        public string? STATO { get; set; }
        public string? CHOSEN_COLOR { get; set; }
    }
}
