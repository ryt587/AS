using _211933M_Assn.Models;
using Microsoft.AspNetCore.Identity;

namespace _211933M_Assn.Services
{
    public class LogService
    {
        private readonly MyDbContext _context;
        public LogService(MyDbContext context)
        {
            _context = context;
        }
        public void AddLog(Log log)
        {
            _context.Logs.Add(log);
            _context.SaveChanges();
        }
        public void RemoveLog(Log log)
        {
            _context.Logs.Remove(log);
            _context.SaveChanges();
        }
    }
}
