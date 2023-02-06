using _211933M_Assn.Models;

namespace _211933M_Assn.Services
{
    public class UserService
    {
        private readonly MyDbContext _context;
        public UserService(MyDbContext context)
        {
            _context = context;
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
    }
}
