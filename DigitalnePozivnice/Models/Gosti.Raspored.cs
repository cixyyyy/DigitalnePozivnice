namespace DigitalnePozivnice.Models.Generated;

public partial class Gosti
{
    /// <summary>Gost kojeg je vlasnik dodao ručno (nije potvrdio preko linka).</summary>
    public bool JeRucni { get; set; }

    public int? StoId { get; set; }

    public virtual Stolovi? Sto { get; set; }
}
