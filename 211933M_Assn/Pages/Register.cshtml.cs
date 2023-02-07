using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;

namespace _211933M_Assn.Pages
{
    //[ValidateReCaptcha]
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
        private EncodingService encoding;
        private UserManager<User> userManager { get; }
        private SignInManager<User> signInManager { get; }
		private readonly RoleManager<IdentityRole> roleManager;
        private readonly LogService logService;
        private IReCaptchaService reCaptchaService { get; }

        public RegisterModel(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailsender,
			RoleManager<IdentityRole> roleManager, EncodingService encoding, LogService logService, IReCaptchaService reCaptchaService)
        {
            _environment = environment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _emailsender = emailsender;
            this.roleManager = roleManager;
            this.encoding = encoding;
            this.logService = logService;
            this.reCaptchaService = reCaptchaService;

		}
        [BindProperty]
        public RUser MyUser { get; set; } = new();
        [BindProperty, Required]
        [AllowedExtension(new string[] { ".jpg" })]
        public IFormFile? Upload { get; set; } = null;
        public static List<User> UserList { get; set; } = new();

        public void OnGet()
        {
            
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(EncodingService.EncodingEmail(MyUser.Email));
                if (user != null)
                {
                    TempData["FlashMessage.Type"] = "danger";
                    TempData["FlashMessage.Text"] = string.Format(MyUser.Email+" has been used");
                    return Page();

                }

                IdentityRole role = await roleManager.FindByIdAsync("Admin");
                if (role == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role admin failed");
                    }
                }

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
				var protector = dataProtectionProvider.CreateProtector("MySecretKey");
				//check employeeID
				User newuser = new User { UserName = MyUser.Name, CCno = protector.Protect(MyUser.CCno), Gender = EncodingService.EncodingMethod(MyUser.Gender), Phone = MyUser.Phone, Address = EncodingService.EncodingMethod(MyUser.Address), Aboutme = EncodingService.EncodingMethod(MyUser.Aboutme), Email = EncodingService.EncodingEmail(MyUser.Email) };
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
                    Log log = new Log { Type = "New Account", Action = newuser.UserName + " makes a new account", LogUser = newuser };
                    logService.AddLog(log);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(newuser);
                    var confirmation = Url.Action("ConfirmEmail", "Account", new { userId = newuser.Id, token }, Request.Scheme);
                    await _emailsender.Execute("Account Verfication", confirmation, MyUser.Email);
                    TempData["FlashMessage.Type"] = "success";
                    TempData["FlashMessage.Text"] = string.Format("Email has been sent for confirmation");
                    return Redirect("/");
                }
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format(result.ToString());
                return Page();
            }
            TempData["FlashMessage.Type"] = "danger";
            TempData["FlashMessage.Text"] = string.Format("Invalid Registration");
            return Page();
        }
    }
}
