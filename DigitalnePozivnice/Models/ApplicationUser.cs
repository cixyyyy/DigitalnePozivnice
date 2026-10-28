
using Microsoft.AspNetCore.Identity;

namespace DigitalnePozivnice.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ImePrezime { get; set; }
    }
}