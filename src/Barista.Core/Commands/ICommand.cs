using System.Threading.Tasks;

namespace Barista.Core.Commands
{
    public interface ICommand
    {
        Task Execute();
    }
}
