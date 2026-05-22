using Gamebot.Models.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfigBacarat
    {
        // (get) Token: 0x060001EA RID: 490 RVA: 0x0001E9C5 File Offset: 0x0001CBC5
        // (set) Token: 0x060001EB RID: 491 RVA: 0x0001E9CD File Offset: 0x0001CBCD
        [JsonPropertyName("globalStopWin")]
        public decimal GlobalStopWin { get; set; }

        // (get) Token: 0x060001EC RID: 492 RVA: 0x0001E9D6 File Offset: 0x0001CBD6
        // (set) Token: 0x060001ED RID: 493 RVA: 0x0001E9DE File Offset: 0x0001CBDE
        [JsonPropertyName("stopWin")]
        public decimal StopWin { get; set; }

        // (get) Token: 0x060001EE RID: 494 RVA: 0x0001E9E7 File Offset: 0x0001CBE7
        // (set) Token: 0x060001EF RID: 495 RVA: 0x0001E9EF File Offset: 0x0001CBEF
        [JsonPropertyName("stopLoss")]
        public decimal StopLoss { get; set; }

        // (get) Token: 0x060001F0 RID: 496 RVA: 0x0001E9F8 File Offset: 0x0001CBF8
        // (set) Token: 0x060001F1 RID: 497 RVA: 0x0001EA00 File Offset: 0x0001CC00
        [JsonPropertyName("safeWin")]
        public decimal SafeWin { get; set; }

        // (get) Token: 0x060001F2 RID: 498 RVA: 0x0001EA09 File Offset: 0x0001CC09
        // (set) Token: 0x060001F3 RID: 499 RVA: 0x0001EA11 File Offset: 0x0001CC11
        [JsonPropertyName("alarm")]
        public decimal Alarm { get; set; }

        // (get) Token: 0x060001F4 RID: 500 RVA: 0x0001EA1A File Offset: 0x0001CC1A
        // (set) Token: 0x060001F5 RID: 501 RVA: 0x0001EA22 File Offset: 0x0001CC22
        public decimal ChangeColor { get; set; }

        // (get) Token: 0x060001F6 RID: 502 RVA: 0x0001EA2B File Offset: 0x0001CC2B
        // (set) Token: 0x060001F7 RID: 503 RVA: 0x0001EA33 File Offset: 0x0001CC33
        [JsonPropertyName("red")]
        public AreaElementConfig AreaRed { get; set; }

        // (get) Token: 0x060001F8 RID: 504 RVA: 0x0001EA3C File Offset: 0x0001CC3C
        // (set) Token: 0x060001F9 RID: 505 RVA: 0x0001EA44 File Offset: 0x0001CC44
        [JsonPropertyName("blu")]
        public AreaElementConfig AreaBlu { get; set; }

        // (get) Token: 0x060001FA RID: 506 RVA: 0x0001EA4D File Offset: 0x0001CC4D
        // (set) Token: 0x060001FB RID: 507 RVA: 0x0001EA55 File Offset: 0x0001CC55
        [JsonPropertyName("areaCentrale")]
        public AreaElementConfig AreaCentrale { get; set; }

        // (get) Token: 0x060001FC RID: 508 RVA: 0x0001EA5E File Offset: 0x0001CC5E
        // (set) Token: 0x060001FD RID: 509 RVA: 0x0001EA66 File Offset: 0x0001CC66
        [JsonPropertyName("areaVincita")]
        public AreaElementConfig AreaVincita { get; set; }

        // (get) Token: 0x060001FE RID: 510 RVA: 0x0001EA6F File Offset: 0x0001CC6F
        // (set) Token: 0x060001FF RID: 511 RVA: 0x0001EA77 File Offset: 0x0001CC77
        [JsonPropertyName("areaPuntare")]
        public AreaElementConfig AreaPuntare { get; set; }

        // (get) Token: 0x06000200 RID: 512 RVA: 0x0001EA80 File Offset: 0x0001CC80
        // (set) Token: 0x06000201 RID: 513 RVA: 0x0001EA88 File Offset: 0x0001CC88
        [JsonPropertyName("areaRaddoppio")]
        public AreaElementConfig AreaRaddoppio { get; set; }

        // (get) Token: 0x06000202 RID: 514 RVA: 0x0001EA91 File Offset: 0x0001CC91
        // (set) Token: 0x06000203 RID: 515 RVA: 0x0001EA99 File Offset: 0x0001CC99
        [JsonPropertyName("areaMazzo")]
        public AreaElementConfig AreaMazzo { get; set; }

        // (get) Token: 0x06000204 RID: 516 RVA: 0x0001EAA2 File Offset: 0x0001CCA2
        // (set) Token: 0x06000205 RID: 517 RVA: 0x0001EAAA File Offset: 0x0001CCAA
        [JsonPropertyName("areaSaldo")]
        public AreaElementConfig AreaSaldo { get; set; }

        // (get) Token: 0x06000206 RID: 518 RVA: 0x0001EAB3 File Offset: 0x0001CCB3
        // (set) Token: 0x06000207 RID: 519 RVA: 0x0001EABB File Offset: 0x0001CCBB
        [JsonPropertyName("fiche1")]
        public AreaElementConfig AreaFiche1 { get; set; }

        // (get) Token: 0x06000208 RID: 520 RVA: 0x0001EAC4 File Offset: 0x0001CCC4
        // (set) Token: 0x06000209 RID: 521 RVA: 0x0001EACC File Offset: 0x0001CCCC
        [JsonPropertyName("fiche5")]
        public AreaElementConfig AreaFiche5 { get; set; }

        // (get) Token: 0x0600020A RID: 522 RVA: 0x0001EAD5 File Offset: 0x0001CCD5
        // (set) Token: 0x0600020B RID: 523 RVA: 0x0001EADD File Offset: 0x0001CCDD
        [JsonPropertyName("fiche25")]
        public AreaElementConfig AreaFiche25 { get; set; }

        // (get) Token: 0x0600020C RID: 524 RVA: 0x0001EAE6 File Offset: 0x0001CCE6
        // (set) Token: 0x0600020D RID: 525 RVA: 0x0001EAEE File Offset: 0x0001CCEE
        [JsonPropertyName("fiche100")]
        public AreaElementConfig AreaFiche100 { get; set; }

        // (get) Token: 0x0600020E RID: 526 RVA: 0x0001EAF7 File Offset: 0x0001CCF7
        // (set) Token: 0x0600020F RID: 527 RVA: 0x0001EAFF File Offset: 0x0001CCFF
        [JsonPropertyName("fiche250")]
        public AreaElementConfig AreaFiche250 { get; set; }

        // (get) Token: 0x06000210 RID: 528 RVA: 0x0001EB08 File Offset: 0x0001CD08
        // (set) Token: 0x06000211 RID: 529 RVA: 0x0001EB10 File Offset: 0x0001CD10
        [JsonPropertyName("fiche500")]
        public AreaElementConfig AreaFiche500 { get; set; }

        // (get) Token: 0x06000212 RID: 530 RVA: 0x0001EB19 File Offset: 0x0001CD19
        // (set) Token: 0x06000213 RID: 531 RVA: 0x0001EB21 File Offset: 0x0001CD21
        [JsonPropertyName("startColor")]
        public string StartColor { get; set; }

        // (get) Token: 0x06000214 RID: 532 RVA: 0x0001EB2A File Offset: 0x0001CD2A
        // (set) Token: 0x06000215 RID: 533 RVA: 0x0001EB32 File Offset: 0x0001CD32
        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        // (get) Token: 0x06000216 RID: 534 RVA: 0x0001EB3B File Offset: 0x0001CD3B
        // (set) Token: 0x06000217 RID: 535 RVA: 0x0001EB43 File Offset: 0x0001CD43
        [JsonPropertyName("martingala")]
        public List<double> Martingala { get; set; }

        // (get) Token: 0x06000218 RID: 536 RVA: 0x0001EB4C File Offset: 0x0001CD4C
        // (set) Token: 0x06000219 RID: 537 RVA: 0x0001EB54 File Offset: 0x0001CD54
        [JsonPropertyName("zoom")]
        public string Zoom { get; set; }

        // (get) Token: 0x0600021A RID: 538 RVA: 0x0001EB5D File Offset: 0x0001CD5D
        // (set) Token: 0x0600021B RID: 539 RVA: 0x0001EB65 File Offset: 0x0001CD65
        [JsonPropertyName("safeWinEnabled")]
        public bool SafeWinEnabled { get; set; }

        // (get) Token: 0x0600021C RID: 540 RVA: 0x0001EB6E File Offset: 0x0001CD6E
        // (set) Token: 0x0600021D RID: 541 RVA: 0x0001EB76 File Offset: 0x0001CD76
        [JsonPropertyName("demoEnabled")]
        public bool DemoEnabled { get; set; }

        // (get) Token: 0x0600021E RID: 542 RVA: 0x0001EB7F File Offset: 0x0001CD7F
        // (set) Token: 0x0600021F RID: 543 RVA: 0x0001EB87 File Offset: 0x0001CD87
        [JsonPropertyName("filterPragmatic")]
        public bool FilterPragmatic { get; set; }

        // (get) Token: 0x06000220 RID: 544 RVA: 0x0001EB90 File Offset: 0x0001CD90
        // (set) Token: 0x06000221 RID: 545 RVA: 0x0001EB98 File Offset: 0x0001CD98
        [JsonPropertyName("martingalaOptions")]
        public List<MartingalaInfoItem> MartingalaOptions { get; set; } = new List<MartingalaInfoItem>();

        // (get) Token: 0x06000222 RID: 546 RVA: 0x0001EBA1 File Offset: 0x0001CDA1
        // (set) Token: 0x06000223 RID: 547 RVA: 0x0001EBA9 File Offset: 0x0001CDA9
        [JsonPropertyName("skipPostSculping")]
        public bool SkipPostSculping { get; set; }

        // (get) Token: 0x06000224 RID: 548 RVA: 0x0001EBB2 File Offset: 0x0001CDB2
        // (set) Token: 0x06000225 RID: 549 RVA: 0x0001EBBA File Offset: 0x0001CDBA
        [JsonPropertyName("indexNamePc")]
        public int IndexNamePc { get; set; }
    }
}
