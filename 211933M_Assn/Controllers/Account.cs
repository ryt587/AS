using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _211933M_Assn.Controllers
{
    public class Account : Controller
    {
        private UserManager<User> _userManager { get; }
        private IWebHostEnvironment _environment;
        private EmailSender _emailsender;
        private SignInManager<User> signInManager { get; }

        public Account(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailsender)
        {
            _environment = environment;
            _userManager = userManager;
            this.signInManager = signInManager;
            _emailsender = emailsender;
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
                return Redirect("/");
            }
            return View();
        }
    }
}
