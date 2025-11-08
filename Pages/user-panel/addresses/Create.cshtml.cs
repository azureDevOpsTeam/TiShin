using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.UserPanel.Addresses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string FullAddress { get; set; } = string.Empty;

            [Required]
            public string PostalCode { get; set; } = string.Empty;

            public bool IsDefault { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (Input.IsDefault)
            {
                var existingDefaults = await _db.UserAddresses
                    .Where(a => a.UserId == user.Id && a.IsDefault)
                    .ToListAsync();
                foreach (var a in existingDefaults)
                {
                    a.IsDefault = false;
                }
            }

            var addr = new UserAddress
            {
                UserId = user.Id,
                FullAddress = Input.FullAddress,
                PostalCode = Input.PostalCode,
                IsDefault = Input.IsDefault
            };

            _db.UserAddresses.Add(addr);
            await _db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}