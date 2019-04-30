using Barista.Core.Commands;

namespace Barista.Core.Data
{
    public interface IPluginMenuItem
    {
        string OriginalTitle { get; }
        string Title { get; }
        bool Trim { get; }
        int Length { get; }
        bool Emojize { get; }
        bool Ansi { get; }
        bool IsCommand { get; }
        ICommand Command { get; }

        bool Refresh { get; }
        string BashScript { get; }
        string[] Params { get; }
        bool Terminal { get; }
        string Href { get; }
    }
}
