using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Schoggifabrik.Data
{
    /// <summary>
    /// Immutable class that is used to store session state.
    /// </summary>
    public class SessionData
    {
        public string RunningTaskId { get; }

        public IImmutableList<string> TaskIds { get; }

        /// <summary>
        /// Defines which problems were solved already.
        /// </summary>
        public int CurrentProblemId { get; }

        [JsonIgnore]
        public bool IsTaskRunning => !string.IsNullOrEmpty(RunningTaskId);

        public SessionData()
        {
            RunningTaskId = null;
            TaskIds = ImmutableList<string>.Empty;
            CurrentProblemId = 0;
        }

        [JsonConstructor]
        public SessionData(string runningTaskId, IList<string> taskIds, int currentProblemId)
        {
            RunningTaskId = runningTaskId;
            TaskIds = taskIds.ToImmutableList();
            CurrentProblemId = currentProblemId;
        }

        private SessionData(string runningTaskId, IImmutableList<string> taskIds, int currentProblemId)
        {
            RunningTaskId = runningTaskId;
            TaskIds = taskIds;
            CurrentProblemId = currentProblemId;
        }

        public SessionData SetRunningTaskId(string runningTaskId)
        {
            if (string.IsNullOrEmpty(runningTaskId)) { throw new ArgumentNullException(); }
            var taskIds = runningTaskId == RunningTaskId ? TaskIds : TaskIds.Add(runningTaskId);
            return new SessionData(runningTaskId, taskIds, CurrentProblemId);
        }

        public SessionData RemoveRunningTaskId() => new SessionData();

        public SessionData AdvanceToNextProblem() =>
            new SessionData(RunningTaskId, TaskIds, Math.Min(CurrentProblemId + 1, Problems.Count - 1));
    }
}
