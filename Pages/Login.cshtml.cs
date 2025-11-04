using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShin.Pages
{
    public class LoginModel(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [BindProperty]
        public LoginInput Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInput
        {
            public string PhoneNumber { get; set; }

            public string Password { get; set; }
        }

        public class SendCodeInputModel
        {
            public string PhoneNumber { get; set; }
        }

        public class LoginCodeInput
        {
            public string PhoneNumber { get; set; }

            public string Code { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _signInManager.PasswordSignInAsync(Input.PhoneNumber, Input.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            ErrorMessage = "ورود ناموفق بود.";
            return Page();
        }

        public async Task<IActionResult> OnPostSendCodeAsync([FromBody] SendCodeInputModel input)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == input.PhoneNumber);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "کاربری با این شماره پیدا نشد." });
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            user.VerificationCode = token;
            user.ExpireCode = DateTime.Now.AddMinutes(2);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["PhoneNumber"] = input.PhoneNumber;

            try
            {
                var api = new Kavenegar.KavenegarApi("784F3738592B59487748785776764B48385A744C2B4E2F702F39654B6247593233754835346274477956673D");
                var result = await api.VerifyLookup(user.PhoneNumber, token, "LoginVerify");
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"خطا در ارسال پیامک: {ex.Message}" });
            }

            return new JsonResult(new { success = true, message = "کد تایید با موفقیت ارسال شد." });
        }

        public async Task<IActionResult> OnPostVerifyCodeAsync([FromBody] LoginCodeInput input)
        {
            if (string.IsNullOrEmpty(input.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "شماره موبایل پیدا نشد." });
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == input.PhoneNumber);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "کاربری با این شماره پیدا نشد." });
            }

            if (user.ExpireCode < DateTime.Now)
            {
                return BadRequest(new { success = false, message = "کد وارد شده معتبر نیست." });
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, input.Code);
            if (!isValid)
            {
                return BadRequest(new { success = false, message = "کد اشتباه است." });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return new JsonResult(new { success = true });
        }
    }
}