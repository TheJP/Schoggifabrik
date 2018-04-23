using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schoggifabrik.Models
{
    public class TaskDetailViewModel
    {
        public TaskViewModel Task { get; }
        public string Source { get; }
        public string CompileOutput { get; }

        public string OutputFirstTestCase { get; }

        public TaskDetailViewModel(TaskViewModel task, string source, string compileOutput, string outputFirstTestCase)
        {
            Task = task;
            Source = source;
            CompileOutput = compileOutput;
            OutputFirstTestCase = outputFirstTestCase;
        }
    }
}
