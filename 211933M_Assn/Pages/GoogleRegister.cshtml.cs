using _211933M_Assn.Models;
using _211933M_Assn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SendGrid.Helpers.Mail;

namespace _211933M_Assn.Pages
{
    [ValidateAntiForgeryToken]
    public class GoogleRegisterModel : PageModel
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

        }
        private IWebHostEnvironment _environment;
        private EmailSender _emailsender;
        private UserManager<User> userManager { get; }
        private SignInManager<User> signInManager { get; }
        private LogService logService;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHttpContextAccessor contxt;
        private readonly UserService userService;

        public GoogleRegisterModel(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, EmailSender emailsender,
            RoleManager<IdentityRole> roleManager, IHttpContextAccessor contxt, LogService logService, UserService userService)
        {
            _environment = environment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _emailsender = emailsender;
            this.roleManager = roleManager;
            this.contxt = contxt;
            this.logService = logService;
            this.userService = userService;
        }
        [BindProperty]
        public RUser MyUser { get; set; } = new();
        [BindProperty]
        [AllowedExtension(new string[] { ".jpg" })]
        public IFormFile? Upload { get; set; } = null;
        public static List<User> UserList { get; set; } = new();
        public void OnGet(string email, string name, string pfp)
        {
            MyUser.Email = email;
            MyUser.Name = name.Replace(" ", "");
            MyUser.ImageURL = pfp;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(EncodingService.EncodingEmail(MyUser.Email));
                if (user != null)
                {
                    TempData["FlashMessage.Type"] = "danger";
                    TempData["FlashMessage.Text"] = string.Format("Email already exist");
                    return Page();
                }
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                //check employeeID
                User newuser = new User { UserName = MyUser.Name, CCno = protector.Protect(MyUser.CCno), Gender = EncodingService.EncodingMethod(MyUser.Gender), Phone = MyUser.Phone, Address = EncodingService.EncodingMethod(MyUser.Address), Aboutme = EncodingService.EncodingMethod(MyUser.Aboutme), Email = EncodingService.EncodingEmail(MyUser.Email), ImageURL = MyUser.ImageURL };
                newuser.EmailConfirmed = true;
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
                var result = await userManager.CreateAsync(newuser);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(newuser, true);
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, "c@c.com"),
                        new Claim(ClaimTypes.Email, "c@c.com")
                        };
                    var i = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                    contxt.HttpContext.Session.SetString("Name", newuser.UserName);
                    contxt.HttpContext.Session.SetString("Email", EncodingService.DecodingEmail(newuser.Email));
                    contxt.HttpContext.Session.SetString("CreditCard", protector.Unprotect(newuser.CCno));
                    TempData["FlashMessage.Type"] = "success";
                    TempData["FlashMessage.Text"] = string.Format("Login Successful");
                    Log log = new Log { Type = "Login", Action = newuser.UserName + "login to account", LogUser = newuser };
                    newuser.Isloggedin = true;
                    var aresult = await userManager.UpdateAsync(newuser);
                    logService.AddLog(log);
                    return Redirect("/");
                }
                TempData["FlashMessage.Type"] = "danger";
                TempData["FlashMessage.Text"] = string.Format("Login Failed");
                return Page();
            }
            TempData["FlashMessage.Type"] = "danger";
            TempData["FlashMessage.Text"] = string.Format("Invalid Login"+ModelState.ToString());
            return Page();
        }
    }
}
