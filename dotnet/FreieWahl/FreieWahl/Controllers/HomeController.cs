using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Application.Tracking;
using FreieWahl.Helpers;
using FreieWahl.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;


namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps]
    public class HomeController : Controller
    {
        private const string ContactAction = "contactform";
        private readonly ILogger _logger;

        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly ISessionCookieProvider _sessionCookieProvider;
        private readonly IMailProvider _mailProvider;
        private readonly IHostingEnvironment _env;
        private readonly ITracker _tracker;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _privateKey;

        public HomeController(ILogger<HomeController> logger,
            IAuthorizationHandler authorizationHandler,
            ISessionCookieProvider sessionCookieProvider,
            IMailProvider mailProvider,
            IHostingEnvironment env,
            IConfiguration configuration,
            ITracker tracker,
            IHttpContextAccessor accessor)
        {
            _logger = logger;
            _authorizationHandler = authorizationHandler;
            _sessionCookieProvider = sessionCookieProvider;
            _mailProvider = mailProvider;
            _env = env;
            _tracker = tracker;
            _accessor = accessor;
            _privateKey = configuration["Google:RecaptchaKey"];
        }

        public async Task<IActionResult> Index(string source)
        {
            _logger.LogInformation("Home page hit!");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _TrackVisit();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            if (source == "logout")
            {
                Response.Cookies.Delete("session");
                return View();
            }

            var auth = await _authorizationHandler.CheckAuthorization(null, Operation.List, Request.Cookies["session"]);

            if (auth.IsAuthorized && source != "redirect")
            {
                Response.Redirect("VotingAdministration/Overview");
            }

            return View();
        }

        private void _TrackVisit()
        {
            try
            {
                _tracker.Track("/Index", _accessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Request.Headers["User-Agent"]
                        .ToString()); // not awaited intentionally. tracking should not slow us down
            }
            catch (Exception)
            {
                // silent catch...
            }
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
        public async Task<IActionResult> SendContactForm(string name, string surname, string email, string message, string recaptchaToken)
        {
            if (!await _CheckRecaptcha(recaptchaToken, ContactAction))
            {
                return Unauthorized();
            }

            message = "Anfrage von '" + name + " " + surname + "' (" + email + "):\r\n" + message;
            await _mailProvider.SendMail(new List<string> { "michael@freiewahl.eu" }, "Kontakt FreieWahl", message, new Dictionary<string, string>());
            return Ok();
        }

        private async Task<bool> _CheckRecaptcha(string recaptcha, string action)
        {
            var client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                {"secret", _privateKey},
                {"response", recaptcha}
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            var result = await response.Content.ReadAsStringAsync();
            var jResult = JObject.Parse(result);
            var success = jResult["success"].Value<bool>();
            var score = jResult["score"].Value<double>();
            if (success && score > .5 && jResult["action"].Value<string>().Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
