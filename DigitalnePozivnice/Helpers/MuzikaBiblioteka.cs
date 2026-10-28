using DigitalnePozivnice.Models.Generated;

namespace DigitalnePozivnice.Helpers;

/// <summary>
/// Biblioteka muzike pozivnice. Sve pjesme su originalni instrumentali (bez autorskih prava
/// trećih strana). Nove pjesme: ubaciti MP3 u wwwroot/audio/biblioteka/ i dodati red ispod.
/// </summary>
public static class MuzikaBiblioteka
{
    public sealed record Pjesma(string Kljuc, string Naziv, string Opis, string Fajl);

    public static readonly IReadOnlyList<Pjesma> Sve = new List<Pjesma>
    {
        new("veseli-dan",    "Veseli dan",     "Vedra, razigrana melodija za slavlje", "veseli-dan.mp3"),
        new("jazz-vecer",    "Džez večer",     "Lagani swing uz klavir i četke", "jazz-vecer.mp3"),
        new("nezni-pocetak", "Nježni početak", "Lagani klavir, topao i miran", "nezni-pocetak.mp3"),
        new("prvi-ples",     "Prvi ples",      "Romantičan valcer na klaviru",  "prvi-ples.mp3"),
        new("zlatni-sat",    "Zlatni sat",     "Meka zvona i zvuk poput sunca pred zalazak", "zlatni-sat.mp3"),
        new("zvjezdana-noc", "Zvjezdana noć",  "Sanjiv, zvjezdan ambijent",     "zvjezdana-noc.mp3"),
        new("ljetna-ljubav", "Ljetna ljubav",  "Nježna akustična gitara",       "ljetna-ljubav.mp3"),
    };

    public const string Podrazumijevana = "veseli-dan";

    public static Pjesma? Nadji(string? kljuc)
        => Sve.FirstOrDefault(p => p.Kljuc == kljuc);

    public static string Url(Pjesma p) => "/audio/biblioteka/" + p.Fajl;

    public const string OpisFoto = "foto";
    public const string OpisMuzika = "muzika";

    public static string MedijUrl(MedijiPozivnice m) => $"/mediji/{m.PozivnicaId}/{m.PutanjaFajla}";

    /// <summary>
    /// URL muzike koja svira na pozivnici ili null (bez muzike).
    /// Save the date paket koristi samo muziku dizajna; Premium/Exclusive mogu birati.
    /// </summary>
    public static string? Razrijesi(Pozivnice p, bool imaRsvp, string? muzikaDizajna, bool demo = false)
    {
        // Demo dizajna uvijek pušta vedru podrazumijevanu pjesmu
        if (demo) return muzikaDizajna ?? Url(Nadji(Podrazumijevana)!);

        if (!imaRsvp) return muzikaDizajna;

        var izbor = p.Muzika;
        if (izbor == "none") return null;

        if (izbor == "moja")
        {
            var m = p.MedijiPozivnices?.FirstOrDefault(x => x.Opis == OpisMuzika);
            if (m != null) return MedijUrl(m);
        }
        else if (izbor != null && izbor.StartsWith("lib:"))
        {
            var pj = Nadji(izbor.Substring(4));
            if (pj != null) return Url(pj);
        }

        // Podrazumijevano: muzika dizajna (ako postoji), inače vedra pjesma iz biblioteke
        return muzikaDizajna ?? Url(Nadji(Podrazumijevana)!);
    }
}
