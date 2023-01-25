using _211933M_Assn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _211933M_Assn.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<User> signInManager;
		public LogoutModel(SignInManager<User> signInManager)
		{
			this.signInManager = signInManager;
		}
		public void OnGet() { }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
			await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
