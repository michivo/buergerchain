using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Models;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace FreieWahl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IJwtAuthentication _authentication;
        private readonly ITimestampService _timestampService;
        private readonly IVotingStore _votingStore;
        private readonly IConfiguration _configuration;
        private static string _lastSigResult;

        public HomeController(ILogger<HomeController> logger,
            IJwtAuthentication authentication,
            ITimestampService timestampService,
            IVotingStore votingStore,
            IConfiguration configuration)
        {
            _logger = logger;
            _authentication = authentication;
            _timestampService = timestampService;
            _votingStore = votingStore;
            _configuration = configuration;
        }

        public async Task<IActionResult> FooBar()
        {
            var headers = Request.Headers;
            string username = "unknown";
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                username = HttpContext.User.Identity.Name;
            }

            var token = _timestampService.GetToken(new byte[] {0xDE, 0xAD, 0xBE, 0xEF});
            var allVotes = await _votingStore.GetAll();

            var model = new FooBarModel(username + headers.Count + token.ToCmsSignedData() + allVotes.Count());

            return View(model);
        }

        public IActionResult GetStuff()
        {
            var result = _authentication.CheckToken(Request.Headers["Authorization"]);
            string username = "---";
            if(result.Success)
            {
                username = result.User.Claims.First(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase)).Value;
            }

            _votingStore.Insert(new StandardVoting
            {
                Creator = username,
                DateCreated = DateTime.UtcNow,
                Description = "Some funky voting",
                Title = "Vote vote vote"
            });

            return Ok(username);
        }

        public IActionResult Index()
        {
            // Sends a message to configured loggers, including the Stackdriver logger.
            // The Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker logger will log all controller actions with
            // log level information. This log is for additional information.
            _logger.LogInformation("Home page hit!");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = _lastSigResult ?? "nuttin";
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
