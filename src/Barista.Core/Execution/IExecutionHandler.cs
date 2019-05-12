using System.Threading.Tasks;
using Barista.Core.Data;

namespace Barista.Core.Execution
{
    public interface IExecutionHandler
    {
        Task Execute(Plugin Plugin);

        Task Execute(Item item);
    }
}
