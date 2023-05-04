using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Api
{
    public class BasicAuth : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        readonly DataContext dataContext;
        public BasicAuth(DataContext dataContext,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock
            ) : base(options, loggerFactory, urlEncoder, clock )
        {
            this.dataContext = dataContext;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic";
            return base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            User? user;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(":");
                var login = credentials.FirstOrDefault();
                var password = credentials.LastOrDefault();

                user = await dataContext.Users.Include(x => x.User_state_id).Where(x => x.Login == login && x.Password == password && x.User_state_id.Code != "blocked").FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new ArgumentException("invalid login or password");
                }
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
