using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Schoggifabrik.Models
{
    public class TaskViewModel
    {
        public string Id { get; }

        public string Name { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TaskStatus Status { get; }

        public string StatusText { get; }

        public TaskViewModel(string id, string name, string statusText, TaskStatus taskStatus)
        {
            Id = id;
            Name = name;
            Status = taskStatus;
            StatusText = statusText;
        }

        public enum TaskStatus { Pending, Success, Error }
    }
}
