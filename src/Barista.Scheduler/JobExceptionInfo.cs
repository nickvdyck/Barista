using System;

namespace Barista.Scheduler
{
    public class JobExceptionInfo
    {
        public string Name { get; set; }
        public Exception Exception { get; set; }
    }
}
