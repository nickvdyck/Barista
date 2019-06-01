using System;
using System.Threading.Tasks;

namespace Barista.Core.Jobs
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
