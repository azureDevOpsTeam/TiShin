using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Coupons
{
    [Authorize(Roles = "Seller")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) { _db = db; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            public string Code { get; set; }

            [Required]
            [RegularExpression("^(percentage|fixed)$")]
            public string Type { get; set; } = "percentage";

            [Required]
            [Range(0.01, double.MaxValue)]
            public decimal Value { get; set; }

            public decimal? MaxAmount { get; set; }

            [Required]
            public DateTime StartDate { get; set; } = DateTime.UtcNow;

            [Required]
            public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);

            public bool IsSingleUse { get; set; } = false;
            public bool IsActive { get; set; } = true;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.StartDate >= Input.EndDate)
            {
                ErrorMessage = "تاریخ پایان باید بعد از تاریخ شروع باشد.";
                return Page();
            }

            var exists = _db.Coupons.Any(c => c.Code == Input.Code);
            if (exists)
            {
                ErrorMessage = "کد کوپن تکراری است.";
                return Page();
            }

            var coupon = new Coupon
            {
                Code = Input.Code.Trim(),
                Percentage = Input.Type == "percentage" ? Input.Value : null,
                FixedAmount = Input.Type == "fixed" ? Input.Value : null,
                MaxAmount = Input.MaxAmount,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                IsSingleUse = Input.IsSingleUse,
                IsActive = Input.IsActive
            };

            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}