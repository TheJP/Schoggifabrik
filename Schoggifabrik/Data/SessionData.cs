using System;

namespace Schoggifabrik.Data
{
    /// <summary>
    /// Immutable class that is used to store session state.
    /// </summary>
    public class SessionData
    {
        public string RunningTaskId { get; }
        public bool IsTaskRunning => !string.IsNullOrEmpty(RunningTaskId);

        public SessionData() => RunningTaskId = null;

        private SessionData(string runningTaskId) => RunningTaskId = runningTaskId;

        public SessionData SetRunningTaskId(string runningTaskId)
        {
            if (string.IsNullOrEmpty(runningTaskId)) { throw new ArgumentNullException(); }
            return new SessionData(runningTaskId);
        }

        public SessionData RemoveRunningTaskId() => new SessionData();
    }
}
