using System;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace JwtAuthentication.WebApp
{
    using System.Linq;
    using System.Security.Principal;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            var jwtCookie = Request.Cookies[".JWTAUTH"];

            string userData;
            if (authCookie != null)
            {
                //Extract the forms authentication cookie
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null)
                {
                    return;
                }

                userData = authTicket.UserData;
            }
            else if (jwtCookie != null)
            {
                userData = jwtCookie.Value;
            }
            else
            {
                return;
            }


            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = GetBytes("ThisIsAnImportantStringAndIHaveNoIdeaIfThisIsVerySecureOrNot!");

            var validationParameters = new TokenValidationParameters()
                                       {
                                           ValidAudience = "http://jwt-sample.com",
                                           ValidIssuer = "http://devkimchi.com",
                                           IssuerSigningToken = new BinarySecretSecurityToken(symmetricKey)
                                       };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(userData, validationParameters, out securityToken);

            // Set the context user
            Context.User = principal;
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}