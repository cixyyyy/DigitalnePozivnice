using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DigitalnePozivnice.Models.Generated;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalnePozivnice.Helpers
{
    /// <summary>Podesavanja slanja emailova (appsettings.Email.json, sekcija "Email").</summary>
    public class EmailOptions
    {
        public bool Enabled { get; set; }
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "Digitalne pozivnice";
        /// <summary>Javna adresa sajta (za linkove u emailovima), npr. https://moj-sajt.com</summary>
        public string SiteUrl { get; set; } = "";
    }

    public interface IEmailService
    {
        /// <summary>Salje email; greska nikad ne ruši stranicu (samo se upiše u log).</summary>
        Task PosaljiAsync(string? kome, string naslov, string htmlTijelo);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailOptions _o;
        private readonly ILogger<EmailService> _log;

        public EmailService(IOptions<EmailOptions> o, ILogger<EmailService> log)
        {
            _o = o.Value;
            _log = log;
        }

        public async Task PosaljiAsync(string? kome, string naslov, string htmlTijelo)
        {
            if (string.IsNullOrWhiteSpace(kome)) return;

            if (!_o.Enabled || string.IsNullOrWhiteSpace(_o.Host) || string.IsNullOrWhiteSpace(_o.FromEmail))
            {
                _log.LogInformation("Email nije poslat (slanje je isključeno). Za: {Kome}, tema: {Tema}", kome, naslov);
                return;
            }

            try
            {
                using var poruka = new MailMessage
                {
                    From = new MailAddress(_o.FromEmail, _o.FromName),
                    Subject = naslov,
                    Body = htmlTijelo,
                    IsBodyHtml = true
                };
                poruka.To.Add(kome.Trim());

                using var klijent = new SmtpClient(_o.Host, _o.Port)
                {
                    EnableSsl = _o.EnableSsl,
                    Timeout = 15000,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                if (!string.IsNullOrWhiteSpace(_o.User))
                    klijent.Credentials = new NetworkCredential(_o.User, _o.Password);

                await klijent.SendMailAsync(poruka);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Slanje emaila nije uspjelo (za {Kome}).", kome);
            }
        }
    }

    /// <summary>Tekstovi emailova (jednostavan, čist izgled; bez vanjskih slika).</summary>
    public static class EmailSadrzaj
    {
        private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

        private static string Okvir(string naslov, string sadrzaj) => $@"
<div style=""background:#f6f1e7;padding:28px 12px;font-family:Georgia,'Times New Roman',serif;color:#2f2a25"">
  <div style=""max-width:520px;margin:0 auto;background:#fffdf8;border:1px solid #e2d6bd;border-radius:14px;padding:32px 28px"">
    <p style=""margin:0 0 6px;font-size:12px;letter-spacing:.3em;text-transform:uppercase;color:#8b7355"">Digitalne pozivnice</p>
    <h1 style=""margin:0 0 18px;font-size:24px;font-weight:500;color:#2f2a25"">{E(naslov)}</h1>
    <div style=""font-size:15px;line-height:1.65"">{sadrzaj}</div>
  </div>
  <p style=""text-align:center;font-size:12px;color:#8b7355;margin:14px 0 0"">Ovo je automatska poruka.</p>
</div>";

        public static (string Naslov, string Tijelo) Dobrodoslica(string? ime, string siteUrl)
        {
            var link = string.IsNullOrWhiteSpace(siteUrl) ? "" : $@"<p><a href=""{E(siteUrl)}"" style=""color:#8b7355"">Otvori sajt</a></p>";
            return ("Dobrodošli!", Okvir("Dobrodošli!",
                $@"<p>Zdravo{(string.IsNullOrWhiteSpace(ime) ? "" : " " + E(ime))},</p>
<p>Vaš nalog je napravljen. Sada možete osmisliti svoju digitalnu pozivnicu, urediti je bez ograničenja i objaviti kad budete spremni.</p>{link}"));
        }

        public static (string Naslov, string Tijelo) NarudzbaKupac(Narudzba n, string nazivPaketa, decimal eur, PlacanjeOptions o)
        {
            var rok = n.DatumKreiranja.AddDays(o.RokUplateDana).ToLocalTime().ToString("dd.MM.yyyy.");
            var sadrzaj = $@"<p>Hvala na narudžbi! Evo podataka za uplatu:</p>
<table style=""border-collapse:collapse;width:100%;font-size:14px"">
<tr><td style=""padding:5px 0;color:#8b7355"">Paket</td><td>{E(nazivPaketa)}</td></tr>
<tr><td style=""padding:5px 0;color:#8b7355"">Iznos</td><td><strong>{n.Iznos:N2} KM</strong> ({eur:N2} €)</td></tr>
<tr><td style=""padding:5px 0;color:#8b7355"">Primalac</td><td>{E(o.Primalac)}<br>{E(o.Adresa)}</td></tr>
<tr><td style=""padding:5px 0;color:#8b7355"">Račun</td><td>{E(o.Racun)} ({E(o.Banka)})</td></tr>
<tr><td style=""padding:5px 0;color:#8b7355"">Poziv na broj</td><td><strong>{E(n.PozivNaBroj)}</strong></td></tr>
<tr><td style=""padding:5px 0;color:#8b7355"">Rok uplate</td><td>{rok}</td></tr>
</table>
<p>Kad uplatu primimo i potvrdimo, javićemo vam se ovim putem, a pozivnicu ćete moći objaviti.</p>
<p>Pitanja: {E(o.KontaktEmail)} {E(o.KontaktTelefon)}</p>";
            return ($"Narudžba #{n.PozivNaBroj}", Okvir($"Narudžba #{n.PozivNaBroj}", sadrzaj));
        }

        public static (string Naslov, string Tijelo) NarudzbaAdmin(Narudzba n, string nazivPaketa, string? emailKupca)
        {
            var sadrzaj = $@"<p>Nova narudžba <strong>#{E(n.PozivNaBroj)}</strong>.</p>
<p>Kupac: {E(emailKupca)}<br>Paket: {E(nazivPaketa)}<br>Iznos: {n.Iznos:N2} KM</p>
<p>Čeka uplatu. Kad uplata stigne, potvrdite je u admin panelu.</p>";
            return ($"Nova narudžba #{n.PozivNaBroj}", Okvir($"Nova narudžba #{n.PozivNaBroj}", sadrzaj));
        }

        public static (string Naslov, string Tijelo) UplataPotvrdjena(Narudzba n, string siteUrl)
        {
            var link = string.IsNullOrWhiteSpace(siteUrl) ? "" : $@"<p><a href=""{E(siteUrl.TrimEnd('/'))}/narudzba/{n.NarudzbaId}"" style=""color:#8b7355"">Pogledaj narudžbu</a></p>";
            return ($"Uplata potvrđena (#{n.PozivNaBroj})", Okvir("Uplata je potvrđena",
                $@"<p>Primili smo uplatu za narudžbu <strong>#{E(n.PozivNaBroj)}</strong>. Hvala vam!</p>
<p>Sada možete objaviti pozivnicu i poslati je gostima.</p>{link}"));
        }
    }
}
