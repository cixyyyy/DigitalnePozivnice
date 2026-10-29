using System.Globalization;
using System.Text.RegularExpressions;
using DigitalnePozivnice.Localization;
using DigitalnePozivnice.Models.Generated;

namespace DigitalnePozivnice.Helpers;

/// <summary>
/// Pomocne metode za prikaz javne pozivnice: izbor dizajna, bezbedne boje i fontovi,
/// formatiranje datuma i linkovi (mapa, kalendar).
/// </summary>
public static class InvitationTheme
{
    public const string DefaultKey = "italija";

    public const string GrupaSaveTheDate = "save-the-date";
    public const string GrupaPremium = "premium";

    public sealed record TemplateInfo(string Partial, string Group);

    // kljuc iz baze (SabloniPozivnica.Kljuc) -> partial sa dizajnom + grupa (za koji paket vazi)
    public static readonly IReadOnlyDictionary<string, TemplateInfo> Templates =
        new Dictionary<string, TemplateInfo>
        {
            // Premium i Exclusive paket
            ["romanticni-vrt"] = new TemplateInfo("~/Views/Pozivnice/Templates/_RomanticniVrt.cshtml", GrupaPremium),
            ["klasicna"] = new TemplateInfo("~/Views/Pozivnice/Templates/_Klasicna.cshtml", GrupaPremium),
            ["italija"] = new TemplateInfo("~/Views/Pozivnice/Templates/_ItalijanskaRomansa.cshtml", GrupaPremium),
            ["fotomat"] = new TemplateInfo("~/Views/Pozivnice/Templates/_Fotomat.cshtml", GrupaPremium),
            ["jazz-vecer"] = new TemplateInfo("~/Views/Pozivnice/Templates/_JazzVecer.cshtml", GrupaPremium),

            // Save the date paket (animacije)
            ["slot-masina"] = new TemplateInfo("~/Views/Pozivnice/Templates/_SlotMasina.cshtml", GrupaSaveTheDate),
            ["koverta"] = new TemplateInfo("~/Views/Pozivnice/Templates/_Koverta.cshtml", GrupaSaveTheDate),
            ["svitak"] = new TemplateInfo("~/Views/Pozivnice/Templates/_Svitak.cshtml", GrupaSaveTheDate),
            ["zavesa"] = new TemplateInfo("~/Views/Pozivnice/Templates/_Zavesa.cshtml", GrupaSaveTheDate)
        };

    // ---------- Paketi ----------

    public static bool JeSaveTheDate(string? oznakaPaketa)
    {
        var o = (oznakaPaketa ?? "").Trim().ToLowerInvariant();
        return o == "save-the-date" || o == "osnovni";
    }

    /// <summary>Koja grupa dizajna vazi za dati paket.</summary>
    public static string GrupaZaPaket(string? oznakaPaketa)
        => JeSaveTheDate(oznakaPaketa) ? GrupaSaveTheDate : GrupaPremium;

    /// <summary>
    /// Save the date paket vidi samo animacije; Premium i Exclusive vide sve dizajne
    /// (animacije + premium dizajne).
    /// </summary>
    public static bool SablonDozvoljen(string? oznakaPaketa, string? kljuc)
        => !JeSaveTheDate(oznakaPaketa) || GrupaSablona(kljuc) == GrupaSaveTheDate;

    public static bool JeExclusive(string? oznakaPaketa)
        => (oznakaPaketa ?? "").Trim().Equals("exclusive", StringComparison.OrdinalIgnoreCase);

    public static string OpisSablona(string? kljuc) => NormalizeKey(kljuc) switch
    {
        "romanticni-vrt" => "Nježni cvjetni motivi i pastelne boje",
        "klasicna" => "Tamna elegancija sa zlatnim detaljima",
        "italija" => "Akvarelna italijanska vila sa lukovima, rukopis koji se ispisuje slovo po slovo",
        "fotomat" => "Pritisni dugme, stari aparat štampa fotografsku traku, pa se otvara kolaž sa vašim fotografijama",
        "jazz-vecer" => "Ručno crtana džez večer sa konobarom, parom i plesačima",
        "slot-masina" => "Povuci ručicu i otkrij datum",
        "koverta" => "Koverta sa pečatom koja se otvara",
        "svitak" => "Kraljevski svitak sa zlatnim kićankama koji se odmotava",
        "zavesa" => "Baršunasta zavjesa koja se razmiče",
        _ => ""
    };

    /// <summary>Samo Premium (interaktivna) i Exclusive imaju potvrdu dolaska.</summary>
    public static bool ImaRsvp(string? oznakaPaketa)
    {
        var o = (oznakaPaketa ?? "").Trim().ToLowerInvariant();
        return o == "premium" || o == "interaktivna" || o == "exclusive";
    }

    private static readonly Regex HexColor =
        new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    private static readonly CultureInfo Sr = CreateCulture();

    private static CultureInfo CreateCulture()
    {
        try { return CultureInfo.GetCultureInfo("sr-Latn-RS"); }
        catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
    }

    // ---------- Dizajn ----------

    public static string NormalizeKey(string? kljuc)
    {
        var k = (kljuc ?? "").Trim().ToLowerInvariant();
        if (k == "tabla") k = "svitak"; // stari dizajn (tabla) je zamijenjen svitkom
        if (k is "prirodna" or "alisa") k = "italija"; // zamijenjeno italijanskom romansom
        return Templates.ContainsKey(k) ? k : DefaultKey;
    }

    public static string TemplatePartial(string? kljuc)
        => Templates[NormalizeKey(kljuc)].Partial;

    public static string GrupaSablona(string? kljuc)
        => Templates[NormalizeKey(kljuc)].Group;

    public static (string Main, string Accent, string Bg) DefaultColors(string? kljuc)
        => NormalizeKey(kljuc) switch
        {
            "romanticni-vrt" => ("#B76E79", "#E8C9C4", "#FFF8F4"),
            "jazz-vecer" => ("#8C2F2B", "#2B2420", "#E2B466"),
            "italija" => ("#5A3E78", "#B08BB8", "#F8F3FA"),
            "fotomat" => ("#8A6A2F", "#C9A961", "#F5EBDD"),
            "slot-masina" => ("#D9627F", "#F6C6D2", "#FBE3EA"),
            "koverta" => ("#A7727D", "#EBD5D0", "#FBF5F1"),
            "svitak" => ("#E48A8F", "#C8973A", "#FBF1E4"),
            "zavesa" => ("#E3C27E", "#8E1F33", "#16090D"),
            _ => ("#C9A961", "#8A7440", "#1F1D1A")
        };

    // ---------- Stil pozadine ----------

    public sealed record StilPoz(string Kljuc, string Naziv);

    public static readonly IReadOnlyList<StilPoz> StiloviPozadine = new[]
    {
        new StilPoz("glatka", "Izvorna"),
        new StilPoz("cista", "Čista"),
        new StilPoz("zive", "Žive boje"),
        new StilPoz("pahulje", "Pahulje"),
        new StilPoz("konfeti", "Konfeti"),
        new StilPoz("mjehurici", "Mjehurići"),
        new StilPoz("srca", "Srca"),
        new StilPoz("latice", "Latice"),
        new StilPoz("zvjezdice", "Zvjezdice"),
        new StilPoz("prasina", "Zlatna prašina"),
        new StilPoz("talasi", "Talasi"),
        new StilPoz("zrake", "Sunčeve zrake")
    };

    public static string StilPozadineOk(string? stil)
    {
        var s = (stil ?? "").Trim().ToLowerInvariant();
        return StiloviPozadine.Any(x => x.Kljuc == s) ? s : "glatka";
    }

    public static string DefaultStil(string? kljuc)
        => NormalizeKey(kljuc) switch
        {
            "slot-masina" => "glatka",
            "zavesa" => "glatka",
            "svitak" => "glatka",
            "fotomat" => "prasina",   // pokretna zlatna prasina je izvorna pozadina (i u demou)
            _ => "glatka"
        };

    /// <summary>Koja uvodna scena se prikazuje: "koverta" ili "none".</summary>
    public static string IntroZa(string? kljuc)
        => GrupaSablona(kljuc) == GrupaPremium && NormalizeKey(kljuc) is not ("jazz-vecer" or "italija" or "fotomat") ? "koverta" : "none";

    // ---------- Bezbedan CSS ----------

    public static string Color(string? value, string fallback)
        => value != null && HexColor.IsMatch(value) ? value : fallback;

    // Tamno ili svetlo pismo, zavisno od svetline pozadine
    public static string TextColor(string bgHex)
    {
        var r = Convert.ToInt32(bgHex.Substring(1, 2), 16);
        var g = Convert.ToInt32(bgHex.Substring(3, 2), 16);
        var b = Convert.ToInt32(bgHex.Substring(5, 2), 16);
        var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        return luminance > 0.55 ? "#2F2A25" : "#F6F1E7";
    }


    // ---------- Sta koji dizajn podrzava (boje / pozadina) ----------

    /// <summary>Dizajni sa ilustrovanom scenom: animirana pozadina (pahulje, srca...) se ne vidi, pa se opcija skriva.</summary>
    public static bool PodrzavaStilPozadine(string? kljuc)
        => NormalizeKey(kljuc) is not ("zavesa" or "jazz-vecer" or "italija");

    /// <summary>Zavjesa ima tamnu scenu, pa boja pozadine ne utice na izgled.</summary>
    public static bool PodrzavaBojuPozadine(string? kljuc)
        => NormalizeKey(kljuc) != "zavesa";

    private static double Nijansa(string hex)
    {
        var r = Convert.ToInt32(hex.Substring(1, 2), 16) / 255.0;
        var g = Convert.ToInt32(hex.Substring(3, 2), 16) / 255.0;
        var b = Convert.ToInt32(hex.Substring(5, 2), 16) / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var d = max - min;
        if (d < 1e-9) return 0;
        double h;
        if (max == r) h = ((g - b) / d) % 6;
        else if (max == g) h = (b - r) / d + 2;
        else h = (r - g) / d + 4;
        h *= 60;
        return h < 0 ? h + 360 : h;
    }

    /// <summary>Pomak nijanse (stepeni) izmedju izabrane i izvorne boje; za CSS hue-rotate.</summary>
    public static string PomakNijanse(string? izabrana, string izvorna)
    {
        if (izabrana == null || !HexColor.IsMatch(izabrana) || !HexColor.IsMatch(izvorna)) return "0";
        var d = Nijansa(izabrana) - Nijansa(izvorna);
        while (d > 180) d -= 360;
        while (d < -180) d += 360;
        return Math.Round(d, 1).ToString(CultureInfo.InvariantCulture);
    }

    public static bool IstaBoja(string? a, string? b)
        => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    public static string FontStack(string? font) => (font ?? "").Trim() switch
    {
        "Arial" => "Arial, Helvetica, sans-serif",
        "Times New Roman" => "'Times New Roman', Times, serif",
        "Cormorant Garamond" => "'Cormorant Garamond', Georgia, serif",
        "Playfair Display" => "'Playfair Display', Georgia, serif",
        "Lora" => "'Lora', Georgia, serif",
        _ => "Georgia, 'Times New Roman', serif"
    };

    // ---------- Sadrzaj ----------

    public static DogadjajiPozivnice? PrviDogadjaj(Pozivnice p)
        => p.DogadjajiPozivnices.OrderBy(d => d.Redosled).FirstOrDefault();

    /// <summary>Vraca (mali natpis iznad, glavni naslov) za pocetni ekran.</summary>
    public static (string Kicker, string Glavno) Glavno(Pozivnice p)
        => string.IsNullOrWhiteSpace(p.ImenaDomacina)
            ? (Loc.T("Pozvani ste"), p.Naslov)
            : (Loc.T("Pozvani ste"), p.ImenaDomacina!.Trim());

    /// <summary>Inicijali za monogram: "Ana i Marko" -> "A &amp; M".</summary>
    public static string Monogram(string? imena)
    {
        if (string.IsNullOrWhiteSpace(imena)) return "♥";

        var preskoci = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "i", "and", "und", "sa" };

        var delovi = imena
            .Split(new[] { ' ', '&', '+', ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !preskoci.Contains(t) && char.IsLetter(t[0]))
            .ToList();

        if (delovi.Count == 0) return "♥";

        var prvo = char.ToUpperInvariant(delovi[0][0]);
        if (delovi.Count == 1) return prvo.ToString();

        var poslednje = char.ToUpperInvariant(delovi[delovi.Count - 1][0]);
        return $"{prvo} & {poslednje}";
    }

    // ---------- Datumi ----------

    // Nazivi dana i mjeseci po jezicima (sr, hr, bs, sl, de, en)
    private static readonly Dictionary<string, string[]> Dani = new()
    {
        ["sr"] = new[] { "nedelja", "ponedeljak", "utorak", "sreda", "četvrtak", "petak", "subota" },
        ["hr"] = new[] { "nedjelja", "ponedjeljak", "utorak", "srijeda", "četvrtak", "petak", "subota" },
        ["bs"] = new[] { "nedjelja", "ponedjeljak", "utorak", "srijeda", "četvrtak", "petak", "subota" },
        ["sl"] = new[] { "nedelja", "ponedeljek", "torek", "sreda", "četrtek", "petek", "sobota" },
        ["de"] = new[] { "Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag" },
        ["en"] = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" }
    };

    private static readonly Dictionary<string, string[]> MjeseciPuni = new()
    {
        ["sr"] = new[] { "januar", "februar", "mart", "april", "maj", "jun", "jul", "avgust", "septembar", "oktobar", "novembar", "decembar" },
        ["hr"] = new[] { "siječnja", "veljače", "ožujka", "travnja", "svibnja", "lipnja", "srpnja", "kolovoza", "rujna", "listopada", "studenoga", "prosinca" },
        ["bs"] = new[] { "januar", "februar", "mart", "april", "maj", "juni", "juli", "august", "septembar", "oktobar", "novembar", "decembar" },
        ["sl"] = new[] { "januarja", "februarja", "marca", "aprila", "maja", "junija", "julija", "avgusta", "septembra", "oktobra", "novembra", "decembra" },
        ["de"] = new[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" },
        ["en"] = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }
    };

    private static string Mjesec(DateTimeOffset d, string lang) => MjeseciPuni[lang][d.Month - 1];

    /// <summary>"6. juna 2027." / "6 June 2027" prema jeziku (bez dana u sedmici).</summary>
    public static string DatumKratko(DateTimeOffset d, string? lang = null)
    {
        lang = Loc.Normalize(lang ?? Loc.Lang);
        return lang switch
        {
            "en" => $"{d.Day} {Mjesec(d, lang)} {d.Year}",
            "de" or "sl" => $"{d.Day}. {Mjesec(d, lang)} {d.Year}",
            _ => $"{d.Day}. {Mjesec(d, lang)} {d.Year}."
        };
    }

    /// <summary>"nedjelja, 6. jun 2027." s danom u sedmici.</summary>
    public static string DatumDugo(DateTimeOffset d, string? lang = null)
    {
        lang = Loc.Normalize(lang ?? Loc.Lang);
        return $"{Dani[lang][(int)d.DayOfWeek]}, {DatumKratko(d, lang)}";
    }

    /// <summary>Npr. "06 JUN 2027" za tablu sa okretanjem slova (bez dijakritika).</summary>
    public static string DatumTabla(DateTimeOffset d, string? lang = null)
    {
        lang = Loc.Normalize(lang ?? Loc.Lang);
        return $"{d.Day:00} {Strings.MonthsAbbr[lang][d.Month - 1].ToUpperInvariant()} {d.Year}";
    }

    public static string Vreme(DateTimeOffset d)
        => d.ToString("HH:mm", Sr);

    public static string IsoZaJs(DateTimeOffset d)
        => d.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);

    // ---------- Linkovi ----------

    /// <summary>Jedan link za kalendar za cijeli dan: od prve do posljednje stavke programa.</summary>
    public static string? CalendarUrlDan(IEnumerable<DogadjajiPozivnice> dogadjaji, string naslov)
    {
        var lista = dogadjaji.Where(d => d.DatumVreme.HasValue)
                             .OrderBy(d => d.DatumVreme).ToList();
        if (lista.Count == 0) return null;

        const string f = "yyyyMMdd'T'HHmmss'Z'";
        var pocetak = lista.First().DatumVreme!.Value.UtcDateTime;
        var kraj = lista.Last().DatumVreme!.Value.UtcDateTime.AddHours(3);

        var program = string.Join("\n", lista.Select(d =>
            $"{d.DatumVreme!.Value:HH:mm} {d.Naziv}"));

        var prvaLokacija = lista.Select(d => $"{d.NazivLokacije} {d.Adresa}".Trim())
                                .FirstOrDefault(x => x.Length > 0) ?? "";

        return "https://calendar.google.com/calendar/render?action=TEMPLATE"
               + "&text=" + Uri.EscapeDataString(naslov)
               + "&details=" + Uri.EscapeDataString(program)
               + "&dates=" + pocetak.ToString(f, CultureInfo.InvariantCulture)
               + "/" + kraj.ToString(f, CultureInfo.InvariantCulture)
               + "&location=" + Uri.EscapeDataString(prvaLokacija);
    }


    public static string? MapsUrl(DogadjajiPozivnice d)
    {
        var upit = $"{d.NazivLokacije} {d.Adresa}".Trim();
        if (string.IsNullOrWhiteSpace(upit)) return null;
        return "https://www.google.com/maps/search/?api=1&query="
               + Uri.EscapeDataString(upit);
    }

    public static string? CalendarUrl(DogadjajiPozivnice d, string naslov)
    {
        if (!d.DatumVreme.HasValue) return null;

        var pocetak = d.DatumVreme.Value.UtcDateTime;
        var kraj = pocetak.AddHours(3);
        const string f = "yyyyMMdd'T'HHmmss'Z'";

        var lokacija = $"{d.NazivLokacije} {d.Adresa}".Trim();

        return "https://calendar.google.com/calendar/render?action=TEMPLATE"
               + "&text=" + Uri.EscapeDataString(naslov)
               + "&details=" + Uri.EscapeDataString(d.Naziv)
               + "&dates=" + pocetak.ToString(f, CultureInfo.InvariantCulture)
               + "/" + kraj.ToString(f, CultureInfo.InvariantCulture)
               + "&location=" + Uri.EscapeDataString(lokacija);
    }
}
