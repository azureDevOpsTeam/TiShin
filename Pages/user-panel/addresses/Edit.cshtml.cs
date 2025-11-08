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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            public string FullAddress { get; set; } = string.Empty;

            [Required]
            public string PostalCode { get; set; } = string.Empty;

            public bool IsDefault { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var addr = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == Id && a.UserId == user.Id);
            if (addr == null) return NotFound();
            Input = new InputModel
            {
                Id = addr.Id,
                FullAddress = addr.FullAddress,
                PostalCode = addr.PostalCode,
                IsDefault = addr.IsDefault
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var addr = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == Input.Id && a.UserId == user.Id);
            if (addr == null) return NotFound();

            if (Input.IsDefault)
            {
                var existingDefaults = await _db.UserAddresses
                    .Where(a => a.UserId == user.Id && a.IsDefault && a.Id != addr.Id)
                    .ToListAsync();
                foreach (var a in existingDefaults)
                {
                    a.IsDefault = false;
                }
            }

            addr.FullAddress = Input.FullAddress;
            addr.PostalCode = Input.PostalCode;
            addr.IsDefault = Input.IsDefault;
            await _db.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}