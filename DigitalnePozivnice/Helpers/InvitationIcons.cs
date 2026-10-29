using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DigitalnePozivnice.Localization;

namespace DigitalnePozivnice.Helpers;

/// <summary>
/// Ikonice koje se prikazuju uz stavke programa (satnicu). Korisnik ih bira pri kreiranju pozivnice;
/// ako nije izabrao, ikonica se pogađa iz naziva i mjesta (Pogodi).
/// Sve su linijske SVG ikonice 24x24, boja prati temu pozivnice (currentColor).
/// </summary>
public static class InvitationIcons
{
    public sealed record Ikona(string Kljuc, string Naziv, string Putanje);

    public const string Podrazumijevana = "srce";

    public static readonly IReadOnlyList<Ikona> Sve = new[]
    {
        new Ikona("crkva-pravoslavna", "Crkva 1",
            "<path d='M12 1.6v4.4M10.6 2.9h2.8M9.8 4.2h4.4'/><path d='M12 6.6c-3.6 2.3-4.4 4.7-3.2 7h6.4c1.2-2.3.4-4.7-3.2-7z'/><path d='M7 13.6V21h10v-7.4M10.4 21v-3.2a1.6 1.6 0 0 1 3.2 0V21M4.5 21h15'/>"),
        new Ikona("crkva-katolicka", "Crkva 2",
            "<path d='M12 1.5v4M10.5 3h3'/><path d='M12 5.6 9 10.6V21M12 5.6l3 5V21M9 10.6h6'/><path d='M9 12.5 5 14.5V21M15 12.5l4 2V21M4 21h16M10.6 21v-3.6a1.4 1.4 0 0 1 2.8 0V21'/>"),
        new Ikona("dzamija", "Džamija",
            "<path d='M6.5 15a5.5 5.5 0 0 1 11 0M5 15h14v6H5zM12 9.5V8'/><path d='M13.3 4.4a2.1 2.1 0 1 0 0 3 1.7 1.7 0 0 1 0-3z'/><path d='M2.2 21V9.5l1.2-2.3 1.2 2.3V21M19.4 21V9.5l1.2-2.3 1.2 2.3V21M10.6 21v-2.4a1.4 1.4 0 0 1 2.8 0V21'/>"),
        new Ikona("maticar", "Matičar / opština",
            "<path d='M3 9.2 12 4l9 5.2zM5 21h14M6.5 11.5v7M10.2 11.5v7M13.8 11.5v7M17.5 11.5v7M4 19.5h16'/>"),
        new Ikona("prsteni", "Prsteni / vjenčanje",
            "<circle cx='9' cy='14.5' r='5'/><circle cx='15.2' cy='14.5' r='5'/><path d='m12 2.8 1.7 2.1-1.7 2.2-1.7-2.2z'/>"),
        new Ikona("restoran", "Restoran",
            "<path d='M6 3v6c0 1.7 1.1 2.7 2.5 2.7S11 10.7 11 9V3M8.5 3v18'/><path d='M17 21V3c-2.6 1.3-4 4.3-4 8.6h4'/>"),
        new Ikona("rostilj", "Roštilj",
            "<path d='M4 10.5h16c0 4.4-3.6 7.3-8 7.3s-8-2.9-8-7.3zM8 17.3 6.6 21M16 17.3 17.4 21M6.5 13.2h11'/><path d='M9 3.2c-1 1.4 1 2.4 0 4M13 3.2c-1 1.4 1 2.4 0 4M17 3.2c-1 1.4 1 2.4 0 4'/>"),
        new Ikona("torta", "Torta",
            "<path d='M4 20.5h16M5 20.5v-6.2h14v6.2M5 17.5c2 1.5 3.6 1.5 5 0s3.4-1.5 5 0 3 1 4 0M12 14.3V11'/><path d='M12 9.6c-1.1-1.2-.1-2.4 0-3.3 1 1 1.1 2.2 0 3.3z'/>"),
        new Ikona("nazdravljanje", "Nazdravljanje",
            "<path d='M6.5 3h11l-.8 6.2a4.7 4.7 0 0 1-9.4 0zM12 14V20M8.5 20.6h7M7 6.4h10'/>"),
        new Ikona("muzika", "Muzika / ples",
            "<path d='M9 17.5V5l11-2v12.5'/><circle cx='6.4' cy='17.6' r='2.6'/><circle cx='17.4' cy='15.6' r='2.6'/>"),
        new Ikona("srce", "Srce",
            "<path d='M12 20.2S4.8 15.9 4.8 10.2a4.3 4.3 0 0 1 7.2-3.1 4.3 4.3 0 0 1 7.2 3.1c0 5.7-7.2 10-7.2 10z'/>"),
        new Ikona("foto", "Fotografisanje",
            "<path d='M3.5 8h3.4l1.7-2.6h6.8L17.1 8h3.4v11.5h-17z'/><circle cx='12' cy='13.6' r='3.8'/>"),
        new Ikona("auto", "Automobil",
            "<path d='M3 16.5V12l2.3-4.7h13.4L21 12v4.5M3 16.5h18M3 16.5v2h3v-2M18 16.5v2h3v-2M5.6 12h12.8'/><circle cx='7.4' cy='14.4' r='.6'/><circle cx='16.6' cy='14.4' r='.6'/>"),
        new Ikona("autobus", "Autobus / prevoz",
            "<path d='M5 17V6.5A2.5 2.5 0 0 1 7.5 4h9A2.5 2.5 0 0 1 19 6.5V17zM5 12h14M7.6 17v2.6M16.4 17v2.6'/><circle cx='8.2' cy='14.6' r='.6'/><circle cx='15.8' cy='14.6' r='.6'/>"),
        new Ikona("cvijece", "Cvijeće",
            "<path d='M12 22v-8.2'/><path d='M12 13.8c-3.6 0-5.6-2.3-5.6-5.2 2.6-.3 4.5.8 5.6 3.1 1.1-2.3 3-3.4 5.6-3.1 0 2.9-2 5.2-5.6 5.2z'/><path d='M12 8.6c-1.6-1.1-1.9-3.2 0-5.2 1.9 2 1.6 4.1 0 5.2z'/>"),
        new Ikona("zvono", "Zvono / ceremonija",
            "<path d='M5.8 17.2c1.3-1.6 1.6-3.2 1.6-6.2a4.6 4.6 0 0 1 9.2 0c0 3 .3 4.6 1.6 6.2zM10 20.2a2 2 0 0 0 4 0M12 4.5V3'/>"),
        new Ikona("poklon", "Poklon",
            "<path d='M4.2 11.2h15.6V20H4.2zM3 8h18v3.2H3zM12 8v12'/><path d='M12 8C11 4.8 7 4.4 7 6.8 7 8 9 8 12 8c3 0 5 0 5-1.2C17 4.4 13 4.8 12 8z'/>"),
        new Ikona("baloni", "Baloni / rođendan",
            "<path d='M9.6 2.8a4.6 5.6 0 0 0-4.6 5.6c0 3.2 2.3 5.4 4.6 5.6 2.3-.2 4.6-2.4 4.6-5.6a4.6 5.6 0 0 0-4.6-5.6zM9.6 14l-.9 1.5h1.8zM9.6 15.5c0 3 3.4 3 3.4 6'/><path d='M17.5 7.6a3.2 4 0 0 1 2.3 3.8c0 2.3-1.6 3.8-3.2 4'/>"),
        new Ikona("krstenje", "Krštenje",
            "<path d='M12 3c3.6 4.3 6.2 7.2 6.2 10.4a6.2 6.2 0 0 1-12.4 0C5.8 10.2 8.4 7.3 12 3zM12 11v6.4M9.4 13.8h5.2'/>"),
        new Ikona("kafa", "Kafa / piće",
            "<path d='M5 9.5h11v5.5a4 4 0 0 1-4 4H9a4 4 0 0 1-4-4zM16 10.4h1.4a2.4 2.4 0 0 1 0 4.8H16M8 3.6v3M11 3.6v3M14 3.6v3'/>"),
        new Ikona("smjestaj", "Smještaj / hotel",
            "<path d='M3 19V6.5M3 14.5h18V19M21 14.5V12A2.5 2.5 0 0 0 18.5 9.5H11v5'/><circle cx='6.8' cy='11.6' r='1.8'/>"),
        new Ikona("zalazak", "Zalazak sunca / napolju",
            "<path d='M3 18.5h18M7 18.5a5 5 0 0 1 10 0M12 6v3M5.4 9.6l1.7 1.7M18.6 9.6l-1.7 1.7M3 14.2h2M19 14.2h2'/>"),
        new Ikona("basta", "Bašta / priroda",
            "<path d='M12 21v-5.2'/><path d='M12 15.8c-4.6 0-6.6-3-6.6-6 0-3.6 3-6.2 6.6-6.2s6.6 2.6 6.6 6.2c0 3-2 6-6.6 6z'/>"),
        new Ikona("okupljanje", "Okupljanje / gosti",
            "<circle cx='9' cy='8' r='3'/><path d='M3.4 20c0-3.3 2.4-5.6 5.6-5.6s5.6 2.3 5.6 5.6'/><path d='M15.6 5.2a2.7 2.7 0 1 1 .8 5.2M17 14.6c2.4.4 3.9 2.4 3.9 5.4'/>"),
        new Ikona("kuca", "Kuća / dom",
            "<path d='m3.8 11.5 8.2-7.2 8.2 7.2M6 10v10.2h12V10M10 20.2v-5.2h4v5.2'/>"),
    };

    private static readonly Dictionary<string, Ikona> Po = Sve.ToDictionary(i => i.Kljuc);

    /// <summary>Pravila za automatski izbor ikonice (regex nad nazivom + mjestom bez dijakritika). Redoslijed je bitan.</summary>
    public static readonly IReadOnlyList<(string Uzorak, string Kljuc)> Pravila = new[]
    {
        (@"dzamij|\bdzemat|mesdzid", "dzamija"),
        (@"katedral|katolick|kapel|zupna|\bzupni|bazilik|franjevac", "crkva-katolicka"),
        (@"crkv|hram|manastir|sabor|bogosluz|liturgij|blagoslov", "crkva-pravoslavna"),
        (@"maticar|opstin|gradsk|civiln|uprava|matic", "maticar"),
        (@"krst", "krstenje"),
        (@"rodjen|rodjendan|balon", "baloni"),
        (@"rostilj|grill|jagnj|prasec|ceval", "rostilj"),
        (@"torta|tortu|kolac|desert|slatk", "torta"),
        (@"restoran|rucak|vecer|svecan|gozba|dorucak|banket|ručak|kafana|konoba|bistro|pecenj", "restoran"),
        (@"nazdrav|zdravic|aperitiv|prijem|sampanj|koktel|cestitk", "nazdravljanje"),
        (@"bend|muzik|orkestar|koncert|dj\b|ples|zabav|igranka|disko|party|svirk", "muzika"),
        (@"foto|slikanj|fotograf", "foto"),
        (@"autobus|prevoz|bus\b|shuttle|kombi", "autobus"),
        (@"polazak|dolazak mlad|auto|limuzin|kolon", "auto"),
        (@"hotel|smjestaj|smestaj|nocenj|prenoc|apartman", "smjestaj"),
        (@"kafa|kafu|kafic|pice|dobrodos|docek|doček", "kafa"),
        (@"cvije|cvet|buket|dekorac", "cvijece"),
        (@"poklon|darov", "poklon"),
        (@"zalaz|napolju|terasa|pristanis|obal|plaz|jezer|ljetni|letnj|bašt|basta|vrt", "zalazak"),
        (@"okupljanj|sastanak|dolazak|doček|docek|svatov|gosti", "okupljanje"),
        (@"vjenc|venc|prsten|obred|ceremon|zakletv|vjerenic|veridb|zarucn", "prsteni"),
    };

    public static string Normaliziraj(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var t = s.ToLowerInvariant().Replace("đ", "dj").Replace("ć", "c").Replace("č", "c").Replace("š", "s").Replace("ž", "z");
        var sb = new StringBuilder();
        foreach (var ch in t.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        return sb.ToString();
    }

    public static Ikona? Nadji(string? kljuc)
        => !string.IsNullOrEmpty(kljuc) && Po.TryGetValue(kljuc, out var i) ? i : null;

    /// <summary>Ukloni nepoznat ključ iz korisničkog unosa.</summary>
    public static string? Ocisti(string? kljuc) => Nadji(kljuc)?.Kljuc;

    /// <summary>
    /// Pogodi ikonicu: najprije iz naziva stavke, pa iz mjesta ("Vjenčanje" u "Crkva Svetog Marka" daje crkvu).
    /// Opšta ikonica prstenja koristi se samo ako ništa preciznije nije pronađeno.
    /// </summary>
    public static string Pogodi(string? naziv, string? mjesto = null)
    {
        var n = Pronadji(Normaliziraj(naziv));
        if (n != null && n != "prsteni") return n;
        var m = Pronadji(Normaliziraj(mjesto));
        if (m != null && m != "prsteni") return m;
        return n ?? m ?? Podrazumijevana;
    }

    private static string? Pronadji(string tekst)
    {
        if (tekst.Length == 0) return null;
        foreach (var (uzorak, kljuc) in Pravila)
            if (Regex.IsMatch(tekst, Normaliziraj(uzorak), RegexOptions.CultureInvariant)) return kljuc;
        return null;
    }

    /// <summary>Ključ koji se prikazuje: izabrani, ili pogođeni ako nije izabran.</summary>
    public static string Odredi(string? izabrana, string? naziv, string? mjesto)
        => Ocisti(izabrana) ?? Pogodi(naziv, mjesto);

    /// <summary>Kompletan &lt;svg&gt; element (aria-hidden).</summary>
    public static string Svg(string kljuc)
    {
        var i = Nadji(kljuc) ?? Po[Podrazumijevana];
        return "<svg viewBox=\"0 0 24 24\" width=\"24\" height=\"24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\" focusable=\"false\">"
               + i.Putanje.Replace('\'', '"') + "</svg>";
    }

    /// <summary>Podaci za izbornik na formi (JSON).</summary>
    public static object ZaFormu() => new
    {
        ikone = Sve.Select(i => new { kljuc = i.Kljuc, naziv = Loc.T(i.Naziv), svg = Svg(i.Kljuc) }),
        pravila = Pravila.Select(p => new[] { Normaliziraj(p.Uzorak), p.Kljuc }),
        podrazumijevana = Podrazumijevana
    };
}
