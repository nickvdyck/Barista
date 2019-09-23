using MediatR;

namespace Barista.Events
{
    public class ExecutePlugin : INotification
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
    }
}
