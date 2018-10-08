using System;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Authentication;
using FreieWahl.Models;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace FreieWahl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly ISessionCookieProvider _sessionCookieProvider;
        private readonly IHostingEnvironment _env;

        public HomeController(ILogger<HomeController> logger,
            IAuthorizationHandler authorizationHandler,
            ISessionCookieProvider sessionCookieProvider,
            IHostingEnvironment env)
        {
            _logger = logger;
            _authorizationHandler = authorizationHandler;
            _sessionCookieProvider = sessionCookieProvider;
            _env = env;
        }

        public async Task<IActionResult> Index(string source)
        {
            _logger.LogInformation("Home page hit!");
            if (source == "logout")
            {
                Response.Cookies.Delete("session");
                return View();
            }

            var user = await _authorizationHandler.GetAuthorizedUser
                (null, Operation.List, Request.Cookies["session"]);

            if (user != null && source != "redirect")
            {
                Response.Redirect("VotingAdministration/Overview");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SessionLogin(string idToken, string csrfToken)
        {
            var sessionCookie = await _sessionCookieProvider.CreateSessionCookie(idToken);

            Response.Cookies.Append("session", sessionCookie.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _env.IsProduction(),
                    Expires = sessionCookie.MaxAge,
                    Path = "/"
                });

            return Ok();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "nuttin";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Error()
        {
            // Log messages with different log levels.
            _logger.LogError("Error page hit!");
            return View();
        }
    }
}
