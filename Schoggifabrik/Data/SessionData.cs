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

        [JsonIgnore]
        public bool IsTaskRunning => !string.IsNullOrEmpty(RunningTaskId);

        public SessionData()
        {
            RunningTaskId = null;
            TaskIds = ImmutableList<string>.Empty;
        }

        [JsonConstructor]
        public SessionData(string runningTaskId, IList<string> taskIds)
        {
            RunningTaskId = runningTaskId;
            TaskIds = taskIds.ToImmutableList();
        }

        private SessionData(string runningTaskId, IImmutableList<string> taskIds)
        {
            RunningTaskId = runningTaskId;
            TaskIds = taskIds;
        }

        public SessionData SetRunningTaskId(string runningTaskId)
        {
            if (string.IsNullOrEmpty(runningTaskId)) { throw new ArgumentNullException(); }
            var taskIds = runningTaskId == RunningTaskId ? TaskIds : TaskIds.Add(runningTaskId);
            return new SessionData(runningTaskId, taskIds);
        }

        public SessionData RemoveRunningTaskId() => new SessionData();
    }
}
