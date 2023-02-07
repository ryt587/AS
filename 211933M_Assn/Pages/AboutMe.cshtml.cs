using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace _211933M_Assn.Pages
{
    [Authorize(Roles ="Admin")]
    public class AboutMeModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
