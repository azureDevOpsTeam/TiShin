using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Entities;

namespace TiShinShop.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "ورود شناسه لازم است")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور لازم است")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        Input.ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var identifier = Input.Identifier?.Trim();
        ApplicationUser? user = null;

        if (string.IsNullOrWhiteSpace(identifier))
        {
            ModelState.AddModelError(string.Empty, "شناسه ورود معتبر نیست");
            return Page();
        }

        if (identifier.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(identifier);
        }
        else if (System.Text.RegularExpressions.Regex.IsMatch(identifier, "^09\\d{9}$"))
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);
        }
        else
        {
            user = await _userManager.FindByNameAsync(identifier);
        }

        if (user == null)
        {
            ErrorMessage = "کاربری با این مشخصات یافت نشد";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(Input.ReturnUrl) && Url.IsLocalUrl(Input.ReturnUrl))
            {
                return LocalRedirect(Input.ReturnUrl);
            }
            return RedirectToPage("/Index");
        }

        ErrorMessage = "ورود ناموفق بود. رمز عبور را بررسی کنید";
        return Page();
    }
}