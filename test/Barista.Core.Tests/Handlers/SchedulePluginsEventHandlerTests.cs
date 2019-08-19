using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Events;
using Barista.Handlers;
using Barista.Common.Jobs;
using Barista.Tests.Builders;
using Moq;
using Xunit;

namespace Barista.Tests.Handlers
{
    public class SchedulePluginsEventHandlerTests
    {
        [Fact]
        public async Task SchedulePluginsEventHandler_Handle_SchedulesANewJobForEachPlugin()
        {
            //Given
            var pluginBuilder = new PluginBuilder();
            var scheduler = new JobScheduler(softRun: true);
            var store = new PluginStore();
            var mediator = new Mock<IMediator>();
            var plugins = new List<Plugin>
            {
                new PluginBuilder().SetFilePath("/root/one.sh").Build(),
                new PluginBuilder().SetFilePath("/root/disabled.sh").Disable().Build(),
                new PluginBuilder().SetFilePath("/root/three.js").Build(),
            };

            store.Update(plugins);

            //When
            var handler = new SchedulePluginsEventHandler(scheduler, store, mediator.Object);
            await handler.Handle(new PluginsUpdatedEvent
            {
                Added = plugins,
                Removed = new List<Plugin>(),
            });

            //Then
            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "one" && s.Disabled == false && s.Immediate == true
            );

            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "disabled" && s.Disabled == true
            );

            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "three" && s.Disabled == false && s.Immediate == true
            );
        }

        [Fact]
        public async Task SchedulePluginsEventHandler_Handle_CorrectlyUpdatesAlreadyRegisteredJobs()
        {
            //Given
            var pluginBuilder = new PluginBuilder();
            var scheduler = new JobScheduler(softRun: true);
            var store = new PluginStore();
            var mediator = new Mock<IMediator>();
            var plugins = new List<Plugin>
            {
                new PluginBuilder().SetFilePath("/root/one.sh").Build(),
                new PluginBuilder().SetFilePath("/root/two.sh").Disable().Build(),
                new PluginBuilder().SetFilePath("/root/three.js").Build(),
            };

            store.Update(plugins);

            //When
            var handler = new SchedulePluginsEventHandler(scheduler, store, mediator.Object);
            await handler.Handle(new PluginsUpdatedEvent { Added = plugins, Removed = new List<Plugin>() });

            store.Update(new List<Plugin>
            {
                new PluginBuilder().SetFilePath("/root/one.sh").Build(),
                new PluginBuilder().SetFilePath("/root/two.sh").Build(),
                new PluginBuilder().SetFilePath("/root/three.js").Disable().Build(),
            });

            await handler.Handle(new PluginsUpdatedEvent { Added = new List<Plugin>(), Removed = new List<Plugin>() });

            //Then
            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "one" && s.Disabled == false
            );

            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "two" && s.Disabled == false && s.Immediate == true
            );

            Assert.Contains(scheduler.AllSchedules, s =>
                s.Name == "three" && s.Disabled == true
            );
        }

        [Fact]
        public async Task SchedulePluginsEventHandler_Handle_RemovesEveryRemovedPluginFromTheScheduler()
        {
            //Given
            var pluginBuilder = new PluginBuilder();
            var scheduler = new JobScheduler(softRun: true);
            var store = new PluginStore();
            var mediator = new Mock<IMediator>();
            var plugins = new List<Plugin>
            {
                pluginBuilder.SetFilePath("/root/one.sh").Build(),
                pluginBuilder.SetFilePath("/root/two.sh").Disable().Build(),
                pluginBuilder.SetFilePath("/root/three.js").Build(),
            };

            store.Update(plugins);

            //When
            var handler = new SchedulePluginsEventHandler(scheduler, store, mediator.Object);
            await handler.Handle(new PluginsUpdatedEvent { Added = store.Plugins, Removed = new List<Plugin>() });

            store.Update(plugins.Skip(1).ToList());
            await handler.Handle(new PluginsUpdatedEvent
            {
                Added = new List<Plugin>(),
                Removed = new List<Plugin> { plugins.First() },
            });

            //Then
            Assert.Equal(2, scheduler.AllSchedules.Count);
            Assert.Contains(scheduler.AllSchedules, s => s.Name == "two");
            Assert.Contains(scheduler.AllSchedules, s => s.Name == "three");
        }

    }
}
