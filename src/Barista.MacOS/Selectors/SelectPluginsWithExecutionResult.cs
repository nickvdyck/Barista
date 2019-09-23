using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Barista.Common.Redux;
using Barista.Data;
using Barista.MacOS.Utils;
using Barista.State;

namespace Barista.MacOS.Selectors
{
    public static class SelectPluginsWithExecutionResult
    {
        public class PluginViewModel : IEquatable<PluginViewModel>
        {
            public string PluginName { get; set; }
            public string TitleAndIcon { get; set; }
            public ImmutableList<ImmutableList<Item>> Items { get; set; }
            public DateTime LastExecution { get; set; }
            public string ExecutedTimeAgo
            {
                get => $"Updated {TimeAgo.Format(LastExecution.ToLocalTime())}";
            }
            public string Colour { get; set; } = string.Empty;

            public bool Equals(PluginViewModel other)
            {
                if (other is null) return false;

                return PluginName == other.PluginName;
            }
            public override bool Equals(object obj) => Equals(obj as PluginViewModel);
            public override int GetHashCode() => (PluginName).GetHashCode();
        }

        public static IObservable<IEnumerable<PluginViewModel>> SelectPlugins(this IReduxStore<BaristaPluginState> store)
        {
            return store
                .Select(state =>
                    state.Plugins
                        .Where(p => !p.Disabled)
                        .Select(p =>
                        {
                            var name = p.Name;
                            var model = new PluginViewModel
                            {
                                PluginName = name,
                            };

                            System.Diagnostics.Debug.WriteLineIf(name == "pomodoro", $"Has execution {state.ExecutionResults.TryGetValue(name, out var _)}");

                            if (state.ExecutionResults.TryGetValue(name, out var execution))
                            {
                                model.LastExecution = execution.LastExecution;
                                if (execution.Success)
                                {
                                    var first = execution.Items.FirstOrDefault().FirstOrDefault();
                                    model.TitleAndIcon = first.Title;
                                    model.Colour = first.Color;
                                    model.Items = execution.Items.Skip(1).ToImmutableList();
                                }
                                else
                                {
                                    model.TitleAndIcon = "⚠️";
                                    model.Items = new List<ImmutableList<Item>>().ToImmutableList();
                                }
                            }
                            else
                            {
                                model.TitleAndIcon = "...";
                                model.LastExecution = DateTime.UtcNow;
                                model.Items = new List<ImmutableList<Item>>().ToImmutableList();
                            }

                            return model;
                        })
                )
                .DistinctUntilChanged();
        }
    }
}
