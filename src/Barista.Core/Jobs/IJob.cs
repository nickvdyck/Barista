using System.Threading.Tasks;

namespace Barista.Core.Jobs
{
    public interface IJob
    {
        Task Execute();
    }
}
