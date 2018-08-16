using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FreieWahl.Controllers
{
    public class PlaygroundController : Controller
    {
        public IActionResult CreateVoting()
        {
            return View();
        }
    }
}