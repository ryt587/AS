using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using OtpNet;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Security.Claims;
using static _211933M_Assn.Pages.LoginModel;

namespace _211933M_Assn.Pages.Shared
{
    public class TwofaModel : PageModel
    {
		public class faUser
		{
			public string twofa { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public bool rmb { get; set; } = false;
        }
		private IWebHostEnvironment _environment;
		private readonly SignInManager<User> signInManager;
		private readonly UserManager<User> userManager;
		private readonly LogService logService;
		private readonly UserService userService;
		private readonly IHttpContextAccessor contxt;
		private EmailSender _emailsender;
		public TwofaModel(IWebHostEnvironment environment, SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, LogService logService, UserService userService, EmailSender emailsender)
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
		public faUser MyUser { get; set; } = new();
		public IActionResult OnGet(string email, string rmb)
        {
            MyUser.email = email;
			MyUser.rmb = rmb == "true" ? true : false;
            return Page();
        }
		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
				var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                //check employeeID
                System.Diagnostics.Debug.WriteLine(MyUser.email);
                User? user = await userManager.FindByEmailAsync(MyUser.email);
				var otpresult = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, MyUser.twofa);
				if (user != null & otpresult)
				{
					await signInManager.SignInAsync(user, MyUser.rmb);
					Log log = new Log { Type = "Login", Action = user.UserName + "login to account", LogUser = user };
					logService.AddLog(log);
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
					user.Isloggedin = true;
					user.TwoFactorEnabled = false;
					var aresult = await userManager.UpdateAsync(user);
					TempData["FlashMessage.Type"] = "success";
					TempData["FlashMessage.Text"] = string.Format("Login Successful");
					return Redirect("/");
				}
				TempData["FlashMessage.Type"] = "danger";
				TempData["FlashMessage.Text"] = string.Format("Email does not exist");
				return Page();
			}
			TempData["FlashMessage.Type"] = "danger";
			TempData["FlashMessage.Text"] = string.Format("Invalid Login");
			return Page();
		}
        //public async Task<IActionResult> OnPostReset()
        //{
            
        //}

    }

}
