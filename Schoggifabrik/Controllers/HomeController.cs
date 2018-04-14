using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Schoggifabrik.Data;
using Schoggifabrik.Models;
using Schoggifabrik.Services;

namespace Schoggifabrik.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger logger;

        public TaskService TaskService { get; }

        public HomeController(ILogger<HomeController> logger, TaskService taskService)
        {
            this.logger = logger;
            TaskService = taskService;
        }

        public IActionResult Index()
        {
            return View(Problems.AsList.First().ToViewModel());
        }

        [HttpPost]
        public IActionResult Index([FromForm] string code, [FromForm] int problemNumber)
        {
            if (problemNumber < 0 || problemNumber >= Problems.AsList.Count) { return BadRequest(); }
            var problem = Problems.AsList[problemNumber];

            var session = HttpContext.GetSessionData();
            if (session.IsTaskRunning && TaskService.TryGetRunningTask(session.RunningTaskId, out var _))
            {
                // There is already a task running for the current user
                return BadRequest();
            }

            try
            {
                var taskId = TaskService.CreateTask(problem, code);
                session = session.SetRunningTaskId(taskId);
                HttpContext.SetSessionData(session);
            }
            catch (TooManyTasksException)
            {
                logger.LogWarning("Tried to create tasks, but there are already too many tasks running");
                return StatusCode(503);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
