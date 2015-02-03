using System.Threading.Tasks;
using System.Web.Mvc;
using JwtAuthentication.WebApp.Models;

namespace JwtAuthentication.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        // GET: Home
        public virtual async Task<ActionResult> Index()
        {
            var vm = new LoginViewModel();
            return View(vm);
        }

        [HttpPost]
        public virtual async Task<ActionResult> Login(LoginViewModel model)
        {
            return RedirectToAction("MyProfile");
        }

        public virtual async Task<ActionResult> MyProfile()
        {
            var vm = new MyProfileViewModel();
            return View(vm);
        }
    }
}