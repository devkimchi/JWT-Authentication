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
            var validated = await this.ValidateAsync(model);
            return RedirectToAction(validated ? "MyProfile" : "Index");
        }

        public virtual async Task<ActionResult> MyProfile()
        {
            var vm = new MyProfileViewModel();
            return View(vm);
        }

        private async Task<bool> ValidateAsync(LoginViewModel model)
        {
            if (String.Equals(model.Email, "test@devkimchi.com", StringComparison.InvariantCultureIgnoreCase) &&
                String.Equals(model.Password, "password", StringComparison.InvariantCultureIgnoreCase))
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
                                          Lifetime = new Lifetime(now, now.AddMinutes(2)),
                                          SigningCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(symmetricKey),
                                                                                      "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                                                                                      "http://www.w3.org/2001/04/xmlenc#sha256"),
                                      };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // http://stackoverflow.com/questions/7217105/asp-net-how-can-i-manually-create-a-authentication-cookie-instead-of-the-defau
                // http://www.codeproject.com/Articles/578374/AplusBeginner-splusTutorialplusonplusCustomplusF

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, // ticket version
                    model.Email, // authenticated username
                    DateTime.Now, // issueDate
                    DateTime.Now.AddMinutes(30), // expiryDate
                    model.RememberMe, // true to persist across browser sessions
                    tokenString, // can be used to store additional user data
                    FormsAuthentication.FormsCookiePath);  // the path for the cookie

                // Encrypt the ticket using the machine key
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                // Add the cookie to the request to save it
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);

                return true;
            }

            return false;
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length*sizeof (char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;

        }
    }
}