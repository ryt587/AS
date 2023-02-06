using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System.Security.Claims;

namespace _211933M_Assn.Controllers
{
    public class Account : Controller
    {
        private UserManager<User> _userManager { get; }
        private IWebHostEnvironment _environment;
        private EmailSender _emailsender;
        private LogService logService;
        private SignInManager<User> signInManager { get; }
		private readonly IHttpContextAccessor contxt;

		public Account(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailsender, IHttpContextAccessor contxt, LogService logService)

		{
            _environment = environment;
            _userManager = userManager;
            this.signInManager = signInManager;
            _emailsender = emailsender;
            this.contxt = contxt;
            this.logService = logService;
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userid, string token)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null || token == null)
            {
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Log log = new Log { Type = "ConfirmEmail", Action = user.UserName + " confirmed his/her ", LogUser = user };
                logService.AddLog(log);
                TempData["FlashMessage.Type"] = "success";
                TempData["FlashMessage.Text"] = string.Format("Email verification successful");
                return Redirect("/Login");
            }
            return View();
        }
        public IActionResult GoogleLogin(string returnUrl = null)
        {
            //await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            //{
            //    RedirectUri = Url.Action("GoogleCallback", "Account", new { returnUrl })
            //});
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }
        public async Task<IActionResult> GoogleCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return Redirect("/Login");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect("/Register");
            }

            // Obtain the user information
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var pfp = info.Principal.FindFirstValue("image");

            // Use the user information for your application logic

            // Redirect to the original URL
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Redirect(returnUrl ?? "/googleregister?email=" + email + "&name=" + name + "&pfp=" + pfp);
            }
            else
            {
                if (!user.Isloggedin)
                {
                    await signInManager.SignInAsync(user, true);
                    var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                    var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                    var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, "c@c.com"),
                            new Claim(ClaimTypes.Email, "c@c.com")
                            };
                    var i = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                    contxt.HttpContext.Session.SetString("Name", user.UserName);
                    contxt.HttpContext.Session.SetString("Email", user.Email);
                    contxt.HttpContext.Session.SetString("CreditCard", protector.Unprotect(user.CCno));
                    Log log = new Log { Type = "Login", Action = user.UserName + "login to account", LogUser = user };
                    logService.AddLog(log);
                    user.Isloggedin = true;
                    var aresult = await _userManager.UpdateAsync(user);
                    TempData["FlashMessage.Type"] = "success";
                    TempData["FlashMessage.Text"] = string.Format("Login Successful");
                    return Redirect("/");
                }
                else
                {
                    TempData["FlashMessage.Type"] = "danger";
                    TempData["FlashMessage.Text"] = string.Format("User is logged in on other devices");
                    return Redirect("/login");
                }
            }
        }
    }
}
