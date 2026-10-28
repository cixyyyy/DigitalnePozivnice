using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalnePozivnice.Localization;

/// <summary>Prevodi tekstualne čvorove i neke atribute u gotovom HTML-u.</summary>
public static class HtmlTranslator
{
    private static readonly Regex Skip = new(
        @"(<script\b[\s\S]*?</script>|<style\b[\s\S]*?</style>|<textarea\b[\s\S]*?</textarea>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TextNode = new(@"(?<=>)([^<>]+)(?=<)", RegexOptions.Compiled);

    private static readonly Regex Attr = new(
        @"(\s(?:placeholder|title|aria-label|alt)=)""([^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TitleTag = new(
        @"<title>([^<]*)</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex Edges = new(@"^(\s*)([\s\S]*?)(\s*)$", RegexOptions.Compiled);

    private const string TitleSuffix = " - Digitalne Pozivnice";

    public static string Translate(string html, string lang)
    {
        var parts = Skip.Split(html);
        var sb = new StringBuilder(html.Length + 256);
        for (var i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 1) { sb.Append(parts[i]); continue; } // script/style/textarea ostaju
            sb.Append(TranslatePart(parts[i], lang));
        }
        return sb.ToString();
    }

    private static string TranslatePart(string part, string lang)
    {
        part = TitleTag.Replace(part, m =>
        {
            var raw = WebUtility.HtmlDecode(m.Groups[1].Value);
            var suffix = raw.EndsWith(TitleSuffix, StringComparison.Ordinal) ? TitleSuffix : "";
            var core = suffix.Length > 0 ? raw[..^suffix.Length] : raw;
            var tr = Loc.Lookup(core, lang);
            return "<title>" + Enc(tr ?? core) + Enc(suffix) + "</title>";
        });

        part = Attr.Replace(part, m =>
        {
            var tr = Loc.Lookup(WebUtility.HtmlDecode(m.Groups[2].Value), lang);
            return tr == null ? m.Value : m.Groups[1].Value + "\"" + Enc(tr) + "\"";
        });

        return TextNode.Replace(part, m =>
        {
            var e = Edges.Match(m.Value);
            var core = e.Groups[2].Value;
            if (core.Length == 0) return m.Value;
            var tr = Loc.Lookup(WebUtility.HtmlDecode(core), lang);
            return tr == null ? m.Value : e.Groups[1].Value + Enc(tr) + e.Groups[3].Value;
        });
    }

    private static string Enc(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
