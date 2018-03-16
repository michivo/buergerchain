using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IJwtAuthentication _authentication;
        private readonly ITimestampService _timestampService;
        private readonly IVotingStore _votingStore;
        private readonly IUserHandler _userHandler;

        public HomeController(ILogger<HomeController> logger,
            IJwtAuthentication authentication,
            ITimestampService timestampService,
            IVotingStore votingStore,
            IUserHandler userHandler)
        {
            _logger = logger;
            _authentication = authentication;
            _timestampService = timestampService;
            _votingStore = votingStore;
            _userHandler = userHandler;
        }

        public async Task<IActionResult> FooBar()
        {
            var headers = Request.Headers;
            string username = "unknown";
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                username = HttpContext.User.Identity.Name;
            }

            var allVotes = await _votingStore.GetAll();

            var model = new FooBarModel(username + headers.Count + allVotes.Count());

            return View(model);
        }

        public IActionResult GetStuff()
        {
            var result = _authentication.CheckToken(Request.Headers["Authorization"]);
            if (result.Success)
            {
                var user = _userHandler.MapUser(result.User);

                _votingStore.Insert(new StandardVoting
                {
                    Creator = user.UserId,
                    DateCreated = DateTime.UtcNow,
                    Description = "Some funky voting",
                    Title = "Vote vote vote"
                });

                return Ok(user.Name);
            }

            return Error();
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
