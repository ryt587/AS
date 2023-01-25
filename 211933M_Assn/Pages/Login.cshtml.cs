using _211933M_Assn.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        public LoginModel(IWebHostEnvironment environment, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _environment = environment;
            this.signInManager = signInManager;
            this.userManager = userManager;
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
                //check employeeID
                User? user = await userManager.FindByEmailAsync(MyUser.Email);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, MyUser.Password, MyUser.RememberMe, true);
                    if (result.Succeeded)
                    { 
						var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, "c@c.com"),
                            new Claim(ClaimTypes.Email, "c@c.com")
                            };
						var i = new ClaimsIdentity(claims, "MyCookieAuth");
						ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
						await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
						return Redirect("/");
                    }
                    return Page();
                }

                return Page();
            }
            return Page();
        }
    }
}
