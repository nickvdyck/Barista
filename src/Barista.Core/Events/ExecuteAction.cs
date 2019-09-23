using Barista.Data;
using MediatR;

namespace Barista.Events
{
    public class ExecuteAction : INotification
    {
        public Item Item { get; set; }
    }
}
