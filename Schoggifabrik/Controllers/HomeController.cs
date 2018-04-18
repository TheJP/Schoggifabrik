using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
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

        /// <summary>Update the session state according to task results.</summary>
        private SessionData UpdateSession(SessionData session)
        {
            if (session.IsTaskRunning && !TaskService.TryGetRunningTask(session.RunningTaskId, out var _))
            {
                if(TaskService.TryGetTask(session.RunningTaskId, out var finishedTask) && finishedTask.Result is TaskData.Success)
                {
                    session = session.AdvanceToNextProblem();
                }
                session = session.RemoveRunningTaskId();
            }
            return session;
        }

        public IActionResult Index([FromRoute] int? id)
        {
            var session = UpdateSession(HttpContext.GetSessionData());
            if (!id.HasValue || id < 0 || id >= Problems.Count || id > session.CurrentProblemId)
            {
                return RedirectToAction(nameof(Index), new { Id = session.CurrentProblemId });
            }

            return View(Problems.AsList[id.Value].ToViewModel());
        }

        [HttpPost]
        public IActionResult SubmitCode([FromForm] string code, [FromRoute] int id)
        {
            var session = UpdateSession(HttpContext.GetSessionData());
            if (id < 0 || id >= Problems.Count || id > session.CurrentProblemId)
            {
                return BadRequest(new { Error = $"Üngültige Problemnummer: '{id}'" });
            }

            var problem = Problems.AsList[id];
            if (session.IsTaskRunning)
            {
                // There is already a task running for the current user
                return BadRequest(new { Error = "Bitte warte mit dem Senden von neuem Code. Deine letzte Einsendung wird noch bewertet." });
            }

            try
            {
                var taskId = TaskService.CreateTask(problem, code);
                session = session.SetRunningTaskId(taskId);
                HttpContext.SetSessionData(session);
                return Json(new { Status = "Task started" });
            }
            catch (TooManyTasksException)
            {
                logger.LogWarning("Tried to create tasks, but there are already too many tasks running");
                return StatusCode(503, new { Error = "Der Server bewertet gerade schon zu viele Einsendungen. Bitte versuche es später nochmals." });
            }
        }

        [HttpGet]
        public JsonResult Results()
        {
            var session = HttpContext.GetSessionData();
            var tasks = session.TaskIds.Reverse()
                .Select(taskId => TaskService.TryGetTask(taskId, out var task) ? task : null)
                .Where(task => task != null)
                .Select(Converter.ToViewModel);
            return Json(tasks);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
