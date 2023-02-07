using _211933M_Assn.Models;
using Azure.Core;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace _211933M_Assn.Services
{
    public class UserService
    {
        private readonly MyDbContext _context;
        private EmailSender emailSender;
        public UserService(MyDbContext context, EmailSender emailSender)
        {
            _context = context;
            this.emailSender = emailSender;
        }
        public void LoginUser(User user)
        {
            user.Isloggedin = true;
            _context.AspNetUsers.Update(user);
            _context.SaveChanges();
        }
        public void LogoutUser(User user)
        {
            user.Isloggedin = false;
            _context.AspNetUsers.Update(user);
            _context.SaveChanges();
        }

        public async Task lockoutemail(User user, string url)
        {
            await emailSender.Execute("Account Lockout Recovery", url, EncodingService.DecodingEmail(user.Email));

        }
    }
}
