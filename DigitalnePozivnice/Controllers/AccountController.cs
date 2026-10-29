
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DigitalnePozivnice.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _email;
        private readonly EmailOptions _emailOpt;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService email,
            IOptions<EmailOptions> emailOpt)
        {
            _email = email;
            _emailOpt = emailOpt.Value;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string imePrezime,
            string email,
            string lozinka,
            string? returnUrl = null,
            bool prihvatam = false)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!prihvatam)
            {
                ModelState.AddModelError("", "Za registraciju morate prihvatiti Uslove korištenja i Politiku privatnosti.");
                return View();
            }

            if (string.IsNullOrWhiteSpace(imePrezime) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(lozinka))
            {
                ModelState.AddModelError("", "Popuni sva polja.");
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                ImePrezime = imePrezime.Trim()
            };

            var result = await _userManager.CreateAsync(user, lozinka);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                var (naslov, tijelo) = EmailSadrzaj.Dobrodoslica(user.ImePrezime, _emailOpt.SiteUrl);
                await _email.PosaljiAsync(user.Email, naslov, tijelo);

                return NazadIliPocetna(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string lozinka,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(lozinka))
            {
                ModelState.AddModelError("", "Unesi email i lozinku.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                email.Trim(),
                lozinka,
                isPersistent: true,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return NazadIliPocetna(returnUrl);
            }

            ModelState.AddModelError("", "Email ili lozinka nisu ispravni.");
            return View();
        }

        // Vraca korisnika tamo odakle je dosao (npr. na kreiranje pozivnice), samo na lokalne adrese.
        private IActionResult NazadIliPocetna(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}