using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using OtpNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Newtonsoft.Json.Linq;
using Hangfire;
using System.Security.Policy;
using System;

namespace _211933M_Assn.Pages
{
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        public class LUser
        {
            [Required]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            public bool RememberMe { get; set; } = false;
        }
        private IWebHostEnvironment _environment;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly LogService logService;
        private readonly UserService userService;
        private readonly IHttpContextAccessor contxt;
        private EmailSender _emailsender;
        public LoginModel(IWebHostEnvironment environment, SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, LogService logService, UserService userService, EmailSender emailsender)
        {
            _environment = environment;
            this.signInManager = signInManager;
            this.userManager = userManager;
            contxt = httpContextAccessor;
            this.logService = logService;
            this.userService = userService;
            _emailsender = emailsender;
        }
        [BindProperty]
        public LUser MyUser { get; set; } = new();
        public static List<User> UserList { get; set; } = new();
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                //check employeeID
                User? user = await userManager.FindByEmailAsync(EncodingService.EncodingEmail(MyUser.Email));
                if (user != null)
                {
                    if (!user.lockout)
                    {
                        if (userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, MyUser.Password).ToString().Equals("Success"))
                        {
                            if (user.Maxpsage >= DateTime.Now)
                            {
                                if (!user.Isloggedin)
                                {
                                    Log log = new Log { Type = "2fa", Action = user.UserName + "created a 2fa", LogUser = user };
                                    logService.AddLog(log);
                                    var otp = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                                    await _emailsender.Execute("OTP", otp, MyUser.Email);
                                    user.TwoFactorEnabled = true;
                                    var aresult = await userManager.UpdateAsync(user);
                                    TempData["FlashMessage.Type"] = "success";
                                    TempData["FlashMessage.Text"] = string.Format("OTP is sent to email");
                                    return Redirect("/twofa?email=" + MyUser.Email + "&rmb=" + MyUser.RememberMe);
                                }
                                else
                                {
                                    TempData["FlashMessage.Type"] = "danger";
                                    TempData["FlashMessage.Text"] = string.Format("User is logged in on other devices");
                                    return Page();
                                }
                            }
                            else
                            {
                                TempData["FlashMessage.Type"] = "danger";
                                TempData["FlashMessage.Text"] = string.Format("Password expired");
                                return Redirect("/changepassword?email=" + MyUser.Email);
                            }
                        }
                        else
                        {
                            var access = user.AccessFailedCount;
                            var result = await signInManager.PasswordSignInAsync(user, MyUser.Password, MyUser.RememberMe, true);
                            if (!result.Succeeded & access == 2)
                            {
                                var callbackUrl = Url.Page(
                                            "/ChangePassword",
                                            pageHandler: null,
                                            values: new { email = EncodingService.DecodingEmail(user.Email) },
                                            protocol: Request.Scheme);
                                user.lockout = true;
                                await userManager.UpdateAsync(user);
                                var client = new BackgroundJobClient();
                                var jobId = client.Schedule( () => _emailsender.Execute("Account Lockout Recovery", callbackUrl, user.Email),TimeSpan.FromMinutes(5));
                                TempData["FlashMessage.Type"] = "danger";
                                TempData["FlashMessage.Text"] = string.Format("Account is lockout");
                                return Page();
                            }
                            else if (!result.Succeeded)
                            {
                                TempData["FlashMessage.Type"] = "danger";
                                TempData["FlashMessage.Text"] = string.Format("Email or Password is incorrect");
                                return Page();

                            }
                            TempData["FlashMessage.Type"] = "danger";
                            TempData["FlashMessage.Text"] = string.Format("Email or Password is incorrect");
                            return Page();
                        }
                    }
                    else
                    {
                        TempData["FlashMessage.Type"] = "danger";
                        TempData["FlashMessage.Text"] = string.Format("User is currently lockout");
                        return Page();
                    }
                }
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format("Email or Password is incorrect");
                return Page();
            }
            TempData["FlashMessage.Type"] = "danger";
            TempData["FlashMessage.Text"] = string.Format("Invalid Login");
            return Page();
        }
    }
}
