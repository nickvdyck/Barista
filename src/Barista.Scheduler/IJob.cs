using System.Threading.Tasks;

namespace Barista.Scheduler
{
    public interface IJob
    {
        Task Execute();
    }
}
