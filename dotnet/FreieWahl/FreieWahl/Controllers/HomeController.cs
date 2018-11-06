using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FreieWahl.Application.Authentication;
using FreieWahl.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps]
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly ISessionCookieProvider _sessionCookieProvider;
        private readonly IHostingEnvironment _env;
        private readonly string _privateKey;

        public HomeController(ILogger<HomeController> logger,
            IAuthorizationHandler authorizationHandler,
            ISessionCookieProvider sessionCookieProvider,
            IHostingEnvironment env,
            IConfiguration configuration)
        {
            _logger = logger;
            _authorizationHandler = authorizationHandler;
            _sessionCookieProvider = sessionCookieProvider;
            _env = env;
            _privateKey = configuration["Google:RecaptchaKey"];
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
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Impressum()
        {
            return View();
        }

        public IActionResult TermsOfUse()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            // Log messages with different log levels.
            _logger.LogError("Error page hit!");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckRecaptcha(string recaptcha)
        {
            var client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "secret", _privateKey },
                { "response", recaptcha }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            var result = await response.Content.ReadAsStringAsync();
            var jResult = JObject.Parse(result);
            var success = jResult["success"].Value<bool>();
            var score = jResult["score"].Value<double>();
            if (success && score > .5)
            {
                return Ok();
            }

            return Unauthorized();
        }
    }
}
