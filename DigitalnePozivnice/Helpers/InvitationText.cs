using DigitalnePozivnice.Localization;
using DigitalnePozivnice.Models.Generated;

namespace DigitalnePozivnice.Helpers;

/// <summary>Tekstovi, predlozi programa i palete boja za pozivnice.</summary>
public static class InvitationText
{
    public sealed record Vrsta(
        string Naziv, string Naslov, string ImenaLabel, string ImenaPrimjer,
        string Kicker, string Linija);

    public sealed record Tema(string Naziv, string Main, string Accent, string Bg);

    public static readonly IReadOnlyList<Vrsta> Vrste = new[]
    {
        new Vrsta("Vjenčanje",  "Naše vjenčanje",       "Imena mladenaca",  "Ana i Marko",        "Pozivamo vas", "da budete dio našeg posebnog dana"),
        new Vrsta("Krštenje",   "Krštenje",             "Ime djeteta",      "Petar",              "Pozivamo vas", "da s nama podijelite radost krštenja"),
        new Vrsta("Rođendan",   "Rođendanska proslava", "Ime slavljenika",  "Milica",             "Pozivamo vas", "da s nama proslavite ovaj poseban rođendan"),
        new Vrsta("Godišnjica", "Naša godišnjica",      "Imena supružnika", "Jelena i Nikola",    "Pozivamo vas", "da s nama proslavite našu godišnjicu"),
        new Vrsta("Prilagođeno", "Proslava",             "Domaćini",         "Porodica Jovanović", "Pozivamo vas", "da budete dio našeg posebnog dana")
    };

    /// <summary>Prepoznaje i stare vrijednosti iz baze ("Venčanje", "Drugo"...).</summary>
    public static Vrsta Nadji(string? naziv)
    {
        var n = (naziv ?? "").Trim().ToLowerInvariant();
        if (n.StartsWith("ven") || n.StartsWith("vjen")) return Vrste[0];
        if (n.StartsWith("krš") || n.StartsWith("krs")) return Vrste[1];
        if (n.StartsWith("rođ") || n.StartsWith("rod")) return Vrste[2];
        if (n.StartsWith("godi")) return Vrste[3];
        return Vrste[4];
    }

    public static string NaslovZa(string? vrsta) => Nadji(vrsta).Naslov;

    /// <summary>"Prilagođeno": kupac sam bira naslov pozivnice.</summary>
    public static bool JeCustom(string? vrsta) => Nadji(vrsta).Naziv == "Prilagođeno";

    /// <summary>Naslov pozivnice: za "Prilagođeno" je to tekst koji je kupac upisao, inace predlozeni naslov vrste.</summary>
    public static string NaslovPozivnice(string? vrsta, string? vlastiti)
    {
        var t = (vlastiti ?? "").Trim();
        return JeCustom(vrsta) && t.Length > 0 ? t : NaslovZa(vrsta);
    }

    /// <summary>Vrsta proslave sa tekstovima na izabranom jeziku (za formu i JavaScript).</summary>
    public static Vrsta Prevedi(Vrsta v, string lang) => v with
    {
        Naslov = Loc.T(v.Naslov, lang),
        ImenaLabel = Loc.T(v.ImenaLabel, lang),
        ImenaPrimjer = Loc.T(v.ImenaPrimjer, lang),
        Kicker = Loc.T(v.Kicker, lang),
        Linija = Loc.T(v.Linija, lang)
    };

    public static (string Kicker, string Linija) Poziv(Pozivnice p)
    {
        var v = Nadji(InvitationTheme.PrviDogadjaj(p)?.VrstaDogadjaja);
        return (v.Kicker, v.Linija);
    }

    public static string PozivLinija(Pozivnice p)
    {
        var (kicker, linija) = Poziv(p);
        return $"{Loc.T(kicker)} {Loc.T(linija)}";
    }

    /// <summary>Predlog programa; nazivi se prevode na jezik (podrazumijevano: jezik zahtjeva).</summary>
    public static IReadOnlyList<(string Naziv, string Vrijeme)> Program(string? vrsta, string? lang = null)
        => ProgramIzvor(vrsta).Select(p => (Loc.T(p.Naziv, lang), p.Vrijeme)).ToList();

    private static IReadOnlyList<(string Naziv, string Vrijeme)> ProgramIzvor(string? vrsta)
        => Nadji(vrsta).Naziv switch
        {
            "Vjenčanje"  => new[] { ("Okupljanje svatova", "12:00"), ("Vjenčanje", "14:00"), ("Svečani ručak", "17:00") },
            "Krštenje"   => new[] { ("Krštenje u crkvi", "11:00"), ("Svečani ručak", "13:30") },
            "Rođendan"   => new[] { ("Dolazak gostiju", "19:00"), ("Proslava", "20:00") },
            "Godišnjica" => new[] { ("Svečana večera", "19:00") },
            _            => new[] { ("Početak proslave", "18:00") }
        };

    /// <summary>Predlozi poruke gostima; prevode se na jezik (podrazumijevano: jezik zahtjeva).</summary>
    public static string[] Poruke(string? vrsta, string? lang = null)
        => PorukeIzvor(vrsta).Select(p => Loc.T(p, lang)).ToArray();

    private static string[] PorukeIzvor(string? vrsta) => Nadji(vrsta).Naziv switch
    {
        "Vjenčanje" => new[]
        {
            "Sa velikom radošću pozivamo vas da s nama podijelite najljepši dan našeg života. Vaše prisustvo učiniće ga potpunim.",
            "Ljubav je najljepša kad se dijeli s onima koje volimo. Zato vas pozivamo da nam se pridružite na dan kada postajemo jedno.",
            "Naša priča dobija novo poglavlje i želimo da ga zajedno počnemo s vama. Radujemo se vašem dolasku."
        },
        "Krštenje" => new[]
        {
            "Sa radošću vas pozivamo da s nama proslavite krštenje našeg djeteta i podijelite ovaj blagoslovljeni dan.",
            "Veliku radost želimo podijeliti s vama. Pozivamo vas na krštenje našeg malog anđela.",
            "Vaše prisustvo učiniće ovaj svečani dan još ljepšim. Radujemo se zajedničkom slavlju."
        },
        "Rođendan" => new[]
        {
            "Još jedna godina, još jedan razlog za slavlje! Pozivamo vas da s nama proslavite ovaj poseban dan.",
            "Dobro društvo je najljepši poklon. Dođite da zajedno napravimo nezaboravnu proslavu.",
            "Spremni smo za muziku, smijeh i dobru zabavu. Čekamo vas!"
        },
        "Godišnjica" => new[]
        {
            "Godine su prošle, a ljubav je ostala. Pozivamo vas da s nama proslavite našu godišnjicu.",
            "Zahvalni smo na svakom zajedničkom danu i želimo da ovaj podijelimo s vama.",
            "Vaše prisustvo učinilo bi ovu proslavu savršenom."
        },
        _ => new[]
        {
            "Sa zadovoljstvom vas pozivamo da nam se pridružite i s nama proslavite ovaj poseban dan.",
            "Radujemo se što ćemo ovaj trenutak podijeliti s vama.",
            "Dođite, družimo se i slavimo zajedno!"
        }
    };

    // ---------- Palete ----------

    private static readonly string[] Tamni = { "klasicna", "zavesa" };

    private static readonly Tema[] SvijetlePalete =
    {
        new("Pudrasto roze", "#D9627F", "#F6C6D2", "#FBE3EA"),
        new("Ruža", "#B3123F", "#F2B8C6", "#FFF1F4"),
        new("Fuksija", "#C2268F", "#F7C3E3", "#FFF2FA"),
        new("Koral", "#E8503F", "#FFC9BE", "#FFF4F0"),
        new("Narandžasta", "#E8730C", "#FFD3A3", "#FFF6EA"),
        new("Zlatna", "#A8801E", "#E9D49A", "#FFFAEC"),
        new("Zelena", "#1F6B4A", "#BFE3CF", "#F2FAF5"),
        new("Smaragdna", "#0F8A6B", "#BDEBDD", "#EFFBF7"),
        new("Tirkizna", "#0E8FA3", "#BDE6EE", "#EFFAFC"),
        new("Svijetloplava", "#2E86D1", "#C4E0F7", "#F1F8FE"),
        new("Plava", "#2748B8", "#C5D1F5", "#F2F5FF"),
        new("Ljubičasta", "#6B33B8", "#DAC7F5", "#F7F1FF"),
        new("Magenta", "#7B2D5B", "#EBC6DC", "#FFF3FA"),
        new("Bordo", "#8E1B3A", "#EBC0CB", "#FFF4F6"),
        new("Terakota", "#B5502F", "#F0CDBD", "#FFF5EF"),
        new("Maslinasta", "#6B7A1F", "#D9E2A6", "#F9FBEA")
    };

    private static readonly Tema[] TamnePalete =
    {
        new("Crna i zlatna", "#D4AF37", "#8A7440", "#14120E"),
        new("Crna i rose gold", "#EFA58A", "#A65A48", "#1A1214"),
        new("Crna i roze", "#FF5C99", "#8E1F52", "#1B0C14"),
        new("Tamnozelena", "#3DDC97", "#176B49", "#09160F"),
        new("Tamnoplava", "#5B9BFF", "#1F4A9E", "#0A1226"),
        new("Ponoćna i zlatna", "#E6C36A", "#2A4590", "#0B1430"),
        new("Bordo i zlatna", "#E8B04A", "#8A1F35", "#1C0A10"),
        new("Tamnoljubičasta", "#B58CFF", "#5A2FA0", "#140C24"),
        new("Grafit", "#F4F1EA", "#7C7770", "#161616"),
        new("Tamna tirkizna", "#2FD3D3", "#0F6F73", "#061718")
    };

    /// <summary>Gotove boje pozadine (svijetle i tamne), uz mogućnost vlastite boje.</summary>
    public static readonly IReadOnlyList<(string Naziv, string Hex)> Pozadine = new (string, string)[]
    {
        ("Bijela",       "#FFFFFF"),
        ("Krem",         "#FFFDF8"),
        ("Šampanjac",    "#FFF4DD"),
        ("Pudrasto roze","#FBE3EA"),
        ("Breskva",      "#FFEDE3"),
        ("Mint",         "#E6F5EE"),
        ("Nebo",         "#E7F1FA"),
        ("Lavanda",      "#F1EAFB"),
        ("Pijesak",      "#F1E7D8"),
        ("Grafit",       "#1F1D1A"),
        ("Bordo",        "#2A0F17"),
        ("Ponoćno plava","#0E1424"),
        ("Šumsko zelena","#10201A"),
        ("Ljubičasta noć","#1B1226")
    };

    public static IReadOnlyList<Tema> Teme(string? kljuc)
    {
        var k = InvitationTheme.NormalizeKey(kljuc);
        var osnovna = InvitationTheme.DefaultColors(k);
        var lista = new List<Tema> { new("Podrazumijevana", osnovna.Main, osnovna.Accent, osnovna.Bg) };

        // prvo paleta koja odgovara dizajnu, pa ostale
        var prve = Tamni.Contains(k) ? TamnePalete : SvijetlePalete;
        var druge = Tamni.Contains(k) ? SvijetlePalete : TamnePalete;

        foreach (var t in prve.Concat(druge))
            if (!lista.Any(x => string.Equals(x.Main, t.Main, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(x.Bg, t.Bg, StringComparison.OrdinalIgnoreCase)))
                lista.Add(t);

        return lista;
    }
}
