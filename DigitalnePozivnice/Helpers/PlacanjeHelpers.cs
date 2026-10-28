using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Models.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalnePozivnice.Helpers
{
    /// <summary>Podaci za uplatu (appsettings.Placanje.json, sekcija "Placanje").</summary>
    public class PlacanjeOptions
    {
        public string Primalac { get; set; } = "";
        public string Adresa { get; set; } = "";
        public string Racun { get; set; } = "";
        public string Banka { get; set; } = "";
        public string KontaktEmail { get; set; } = "";
        public string KontaktTelefon { get; set; } = "";
        public int RokUplateDana { get; set; } = 7;
    }

    public static class Cijene
    {
        /// <summary>Fiksni kurs: 1 EUR = 1,95583 KM.</summary>
        public const decimal KmPoEuru = 1.95583m;

        public static decimal Eur(decimal km) => Math.Round(km / KmPoEuru, 2);
    }

    public static class PlacanjeHelper
    {
        /// <summary>Pozivnica se smije objaviti ako je paket besplatan ili postoji placena narudzba.</summary>
        public static async Task<bool> MozeObjavitiAsync(DigitalnePozivniceScaffoldContext ctx, int pozivnicaId, decimal cijenaPaketa)
        {
            if (cijenaPaketa <= 0) return true;

            return await ctx.Narudzbe.AnyAsync(n =>
                n.PozivnicaId == pozivnicaId && n.Status == NarudzbaStatus.Placeno);
        }

        /// <summary>Admin je otkazao narudzbu i nema placene: korisnik ne moze ponovo platiti ni objaviti.</summary>
        public static async Task<bool> JeBlokiranaAsync(DigitalnePozivniceScaffoldContext ctx, int pozivnicaId)
        {
            var poNarudzbama = await ctx.Narudzbe
                .Where(n => n.PozivnicaId == pozivnicaId)
                .Select(n => n.Status)
                .ToListAsync();

            return poNarudzbama.Contains(NarudzbaStatus.Ponisteno) && !poNarudzbama.Contains(NarudzbaStatus.Placeno);
        }
    }

    /// <summary>Admin je ko je u ulozi "Admin" ili mu je email u appsettings (Admin:Emails).</summary>
    public class AdminProvjera
    {
        private readonly string[] _emailovi;

        public AdminProvjera(IConfiguration config)
        {
            _emailovi = config.GetSection("Admin:Emails").Get<string[]>() ?? Array.Empty<string>();
        }

        public bool JeAdminEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email)
            && _emailovi.Any(e => string.Equals(e?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));

        public bool JeAdmin(ClaimsPrincipal? korisnik)
        {
            if (korisnik?.Identity?.IsAuthenticated != true) return false;
            if (korisnik.IsInRole("Admin")) return true;

            return JeAdminEmail(korisnik.Identity.Name);
        }
    }

    public class AdminRequirement : IAuthorizationRequirement { }

    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly AdminProvjera _provjera;

        public AdminHandler(AdminProvjera provjera) => _provjera = provjera;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            if (_provjera.JeAdmin(context.User)) context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
