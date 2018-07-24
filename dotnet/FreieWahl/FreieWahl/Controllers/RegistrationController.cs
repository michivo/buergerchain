using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IVotingTokenHandler _tokenHandler;

        public RegistrationController(IVotingTokenHandler tokenHandler)
        {
            _tokenHandler = tokenHandler;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register()
        {

        }
    }
}
