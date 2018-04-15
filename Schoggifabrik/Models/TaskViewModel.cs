using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Schoggifabrik.Models
{
    public class TaskViewModel
    {
        public string Name { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TaskStatus Status { get; }

        public string StatusText { get; }

        public TaskViewModel(string name, string statusText, TaskStatus taskStatus)
        {
            Name = name;
            Status = taskStatus;
            StatusText = statusText;
        }

        public enum TaskStatus { Pending, Success, Error }
    }
}
