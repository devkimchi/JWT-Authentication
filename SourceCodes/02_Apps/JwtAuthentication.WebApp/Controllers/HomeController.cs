using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using JwtAuthentication.WebApp.Models;

namespace JwtAuthentication.WebApp.Controllers
{
    [Authorize]
    public partial class HomeController : Controller
    {
        // GET: Home
        [AllowAnonymous]
        public virtual async Task<ActionResult> Index()
        {
            var vm = new LoginViewModel();
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual async Task<ActionResult> LoginForm(LoginViewModel model)
        {
            var validated = await this.ValidateAsync(model, "form");
            return RedirectToAction(validated ? "MyProfile" : "Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual async Task<ActionResult> LoginJwt(LoginViewModel model)
        {
            var validated = await this.ValidateAsync(model, "jwt");
            return RedirectToAction(validated ? "MyProfile" : "Index");
        }

        //[Authorize(Roles = "Admin")]
        public virtual async Task<ActionResult> MyProfile()
        {
            var vm = new MyProfileViewModel() { Name = User.Identity.Name };
            return View(vm);
        }

        private async Task<bool> ValidateAsync(LoginViewModel model, string loginType)
        {
            if (!String.Equals(model.Email, "test@devkimchi.com", StringComparison.InvariantCultureIgnoreCase) ||
                !String.Equals(model.Password, "password", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var validated = await this.CreateCookie(model, loginType);
            return validated;
        }

        private async Task<bool> CreateCookie(LoginViewModel model, string loginType)
        {
            // https://pfelix.wordpress.com/2012/11/27/json-web-tokens-and-the-new-jwtsecuritytokenhandler-class/
            // http://blog.codeinside.eu/2014/04/06/create-and-validate-own-json-web-tokens-jwts/
            // http://stackoverflow.com/questions/22587992/jwt-and-web-api-jwtauthforwebapi-looking-for-an-example

            var now = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = GetBytes("ThisIsAnImportantStringAndIHaveNoIdeaIfThisIsVerySecureOrNot!");
            var tokenDescriptor = new SecurityTokenDescriptor
                                      {
                                          Subject = new ClaimsIdentity(new Claim[]
                                                                           {
                                                                               new Claim(ClaimTypes.Name, "DevKimchi"),
                                                                               new Claim(ClaimTypes.Role, "User"),
                                                                           }),
                                          TokenIssuerName = "http://devkimchi.com",
                                          AppliesToAddress = "http://jwt-sample.com",
                                          Lifetime = new Lifetime(now, now.AddMinutes(30)),
                                          SigningCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(symmetricKey),
                                                                                      "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                                                                                      "http://www.w3.org/2001/04/xmlenc#sha256"),
                                      };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var cookie = new HttpCookie(".JWTAUTH", tokenString) { HttpOnly = true, };
            if (loginType == "form")
            {
                // http://stackoverflow.com/questions/7217105/asp-net-how-can-i-manually-create-a-authentication-cookie-instead-of-the-defau
                // http://www.codeproject.com/Articles/578374/AplusBeginner-splusTutorialplusonplusCustomplusF

                var ticket = new FormsAuthenticationTicket(
                                 1,                  // ticket version
                                 model.Email,        // authenticated username
                                 now,                // issueDate
                                 now.AddMinutes(30), // expiryDate
                                 model.RememberMe,   // true to persist across browser sessions
                                 tokenString,        // can be used to store additional user data
                                 FormsAuthentication.FormsCookiePath);  // the path for the cookie

                // Encrypt the ticket using the machine key
                var encryptedTicket = FormsAuthentication.Encrypt(ticket);

                // Add the cookie to the request to save it
                cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) { HttpOnly = true, };
            }

            Response.Cookies.Add(cookie);

            return true;
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;

        }
    }
}