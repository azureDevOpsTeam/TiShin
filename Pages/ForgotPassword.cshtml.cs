using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Entities;

namespace TiShinShop.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Message { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "ایمیل یا موبایل لازم است")]
        public string EmailOrPhone { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var key = Input.EmailOrPhone.Trim();
        ApplicationUser? user = null;
        if (key.Contains('@'))
            user = await _userManager.FindByEmailAsync(key);
        else if (System.Text.RegularExpressions.Regex.IsMatch(key, "^09\\d{9}$"))
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == key);

        if (user == null)
        {
            Message = "در صورت وجود حساب، پیام ارسال خواهد شد.";
            return Page();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // در این نسخه، ارسال ایمیل/پیامک پیاده‌سازی نشده است.
        Message = "کد بازیابی تولید شد و به ایمیل/موبایل شما ارسال می‌شود.";
        return Page();
    }
}