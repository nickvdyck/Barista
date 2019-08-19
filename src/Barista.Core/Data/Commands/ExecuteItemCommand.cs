using Barista.Common;

namespace Barista.Data.Commands
{
    public class ExecuteItemCommand : ICommand
    {
        public Item Item { get; set; }
    }
}
