using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace _211933M_Assn.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ChangePasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// [Required]
            [Required]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }

        public IActionResult OnGet(string email)
        {
            System.Diagnostics.Debug.WriteLine(email);
            Input.Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Redirect("/error/404");
            }

            var user = await _userManager.FindByEmailAsync(EncodingService.EncodingEmail(Input.Email));
            if (user == null)
            {
                // Don't reveal that the user does not exist
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format("User doesn't exist"); ;
                return Redirect("/Users/ForgotPassword/AskEmail");
            }
            PasswordVerificationResult hash = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password);
            PasswordVerificationResult? hash2 = null;
            if (user.prevps!= null)
            {
                hash2 = _userManager.PasswordHasher.VerifyHashedPassword(user, user.prevps, Input.Password);
            }
            else
            {
                hash2 = null;
            }
            if (hash.ToString().Equals("Success") | hash2.ToString().Equals("Success"))
            {
                user.lockout = false;
                await _userManager.UpdateAsync(user);
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format("Password has been used on this account"); ;
                return Page();
            }
            if (user.Minpsage >= DateTime.Now)
            {
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format("Password is recently changed"); ;
                return Page();
            }
            user.prevps = user.PasswordHash.ToString();
            await _userManager.UpdateAsync(user);
            var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.Password);
            if (result.Succeeded)
            {
                user.Minpsage = DateTime.Now.AddDays(1);
                user.Maxpsage = DateTime.Now.AddDays(30);
                var aresult = await _userManager.UpdateAsync(user);
                TempData["FlashMessage.Type"] = "success";
                TempData["FlashMessage.Text"] = string.Format("Password have been reset"); ;
                return Redirect("/");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Redirect("/error/403");
        }
    }
}
