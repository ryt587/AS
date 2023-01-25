using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;

namespace _211933M_Assn.Pages
{
	[ValidateAntiForgeryToken]
	public class RegisterModel : PageModel
    {
        public class RUser
        {
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            public string CCno { get; set; } = string.Empty;
            [Required]
            public string Gender { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; } = string.Empty;

            [Required, RegularExpression(@"^(6|8|9)\d{7}$", ErrorMessage = "Invalid Phone number.")]
            [DataType(DataType.PhoneNumber)]
            public int Phone { get; set; }

            [Required, MaxLength(100)]
            public string Address { get; set; } = string.Empty;

            [MaxLength(50)]
            [DataType(DataType.ImageUrl)]
            public string? ImageURL { get; set; } = string.Empty;
            [Required]
            public string Aboutme { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirmation Password is required.")]
            [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
            [NotMapped]
            public string CfmPassword { get; set; } = string.Empty;

        }
        private IWebHostEnvironment _environment;
        private EmailSender _emailsender;
        private UserManager<User> userManager { get; }
        private SignInManager<User> signInManager { get; }
		private readonly RoleManager<IdentityRole> roleManager;

		public RegisterModel(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailsender,
			RoleManager<IdentityRole> roleManager)
        {
            _environment = environment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _emailsender = emailsender;
            this.roleManager = roleManager;

		}
        [BindProperty]
        public RUser MyUser { get; set; } = new();
        [BindProperty]
        public IFormFile? Upload { get; set; }
        public static List<User> UserList { get; set; } = new();

        public void OnGet()
        {
        }
		public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(MyUser.Email);
                if (user != null)
                {
                    return Page();

                }
				var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
				var protector = dataProtectionProvider.CreateProtector("MySecretKey");
				//check employeeID
				User newuser = new User { UserName = MyUser.Name, CCno = protector.Protect(MyUser.CCno), Gender = MyUser.Gender, Phone = MyUser.Phone, Address = MyUser.Address, Aboutme = MyUser.Aboutme, Email = MyUser.Email};
                if (Upload != null)
                {
                    if (Upload.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Upload",
                        "File size cannot exceed 2MB.");
                        return Page();
                    }
                    var uploadsFolder = "uploads";
                    var imageFile = Guid.NewGuid() + Path.GetExtension(Upload.FileName);
                    var imagePath = Path.Combine(_environment.ContentRootPath, "wwwroot", uploadsFolder, imageFile);
                    using var fileStream = new FileStream(imagePath,
                    FileMode.Create);
                    await Upload.CopyToAsync(fileStream);
                    newuser.ImageURL = string.Format("/{0}/{1}", uploadsFolder, imageFile);
                }
                var result = await userManager.CreateAsync(newuser, MyUser.Password);
                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(newuser);
                    var confirmation = Url.Action("ConfirmEmail", "Account", new { userId = newuser.Id, token }, Request.Scheme);
                    await _emailsender.Execute("Account Verfication", confirmation, MyUser.Email);
                    return Redirect("/");
                }
                return Page();
            }
            return Page();
        }
    }
}
