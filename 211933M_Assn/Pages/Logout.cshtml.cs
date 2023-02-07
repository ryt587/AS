using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _211933M_Assn.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly LogService logService;
        private readonly UserService userService;
        public LogoutModel(SignInManager<User> signInManager, UserManager<User> userManager, LogService logService, UserService userService)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.logService = logService;
			this.userService = userService;
		}
		public void OnGet() { }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
			User user  = await userManager.GetUserAsync(HttpContext.User);
            user.Isloggedin = false;
            await userManager.UpdateAsync(user);
            Log log = new Log { Type = "Logout", Action = user.UserName + " logout", LogUser = user };
			logService.AddLog(log);
            await signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["FlashMessage.Type"] = "success";
            TempData["FlashMessage.Text"] = string.Format("Logout");
            return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
