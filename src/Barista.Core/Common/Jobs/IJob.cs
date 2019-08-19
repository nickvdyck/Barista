using System.Threading.Tasks;

namespace Barista.Common.Jobs
{
    internal interface IJob
    {
        Task Execute();
    }
}
