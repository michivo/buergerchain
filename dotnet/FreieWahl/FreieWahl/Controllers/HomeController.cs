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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace FreieWahl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        
        private readonly IAuthorizationHandler _authorizationHandler;

        public HomeController(ILogger<HomeController> logger,
            IAuthorizationHandler authorizationHandler)
        {
            _logger = logger;
            _authorizationHandler = authorizationHandler;
        }

        public async Task<IActionResult> Index(string source)
        {
            _logger.LogInformation("Home page hit!");
            var user = await _authorizationHandler.GetAuthorizedUser
                (null, Operation.List, Request.Cookies["token"]);

            if (user != null && source != "redirect")
            {
                Response.Redirect("VotingAdministration/Overview");
            }

            return View();
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
