using Barista.Actions;
using Barista.Common.Redux;
using Barista.Data;

namespace Barista.State
{
    public class PluginReducer : IReducer<BaristaPluginState>
    {
        public BaristaPluginState Execute(BaristaPluginState state, IAction inputAction)
        {
            switch (inputAction)
            {
                case PluginsUpdatedAction action:
                {
                    return new BaristaPluginState
                    {
                        Plugins = action.Plugins,
                        ExecutionResults = state.ExecutionResults
                    };
                }

                case PluginExecutedAction action:
                {

                    var results = state.ExecutionResults.ToBuilder();

                    results[action.Name] = new PluginExecution
                    {
                        Items = action.Items,
                        Success = action.Success,
                        LastExecution = action.TimeStamp,
                    };

                    return new BaristaPluginState
                    {
                        Plugins = state.Plugins,
                        ExecutionResults = results.ToImmutable(),
                    };
                }

                default:
                    return state;
            }
        }
    }
}
