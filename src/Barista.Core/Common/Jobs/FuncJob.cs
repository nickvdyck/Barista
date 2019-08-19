using System;
using System.Threading.Tasks;

namespace Barista.Common.Jobs
{
    internal class FuncJob : IJob
    {
        private readonly Func<Task> _action;
        public FuncJob(Func<Task> action)
        {
            _action = action;
        }

        public Task Execute() => _action.Invoke();
    }
}
