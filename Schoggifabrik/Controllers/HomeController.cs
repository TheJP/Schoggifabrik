using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Schoggifabrik.Data;
using Schoggifabrik.Models;

namespace Schoggifabrik.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger logger;

        public HomeController(ILogger<HomeController> logger) => this.logger = logger;

        public IActionResult Index()
        {
            return View(Tasks.AsList.First());
        }

        [HttpPost]
        public IActionResult Index([FromForm] string code)
        {
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
