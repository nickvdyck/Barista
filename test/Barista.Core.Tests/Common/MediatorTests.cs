using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Moq;
using Xunit;

namespace Barista.Tests.Common
{
    public class MediatorTests
    {
        public class Hello : IEvent
        {
            public string Message { get; set; } = "Hello World";
        }

        public class Ping : IEvent
        {
            public string Message { get; set; } = "Ping";
        }


        [Fact]
        public async Task Mediator_Publish_ShouldCallARegisteredHandlerForTheGivenEvent()
        {
            // Given
            var hello = new Hello();
            var mockEventHandler = new Mock<IEventHandler<Hello>>();
            object provider(Type type) => new List<IEventHandler<Hello>> { mockEventHandler.Object };
            var mediator = new Mediator(provider);

            // When
            await mediator.Publish(hello);

            // Then
            mockEventHandler.Verify(m => m.Handle(hello, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Mediator_Publish_ShouldCallEachHandlerOneByOne()
        {
            // Given
            var ping = new Ping();
            var firstHandler = new Mock<IEventHandler<Ping>>();
            var secondHandler = new Mock<IEventHandler<Ping>>();
            object provider(Type type) => new List<IEventHandler<Ping>> { firstHandler.Object, secondHandler.Object };
            var mediator = new Mediator(provider);

            var firstHandlerCalled = DateTime.MaxValue;
            var secondHandlerCalled = DateTime.MaxValue;

            firstHandler
                .Setup(m => m.Handle(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
                .Callback(() => firstHandlerCalled = DateTime.UtcNow)
                .Returns(Task.CompletedTask);

            secondHandler
                .Setup(m => m.Handle(It.IsAny<Ping>(), It.IsAny<CancellationToken>()))
                .Callback(() => secondHandlerCalled = DateTime.UtcNow)
                .Returns(Task.CompletedTask);


            // When
            await mediator.Publish(ping);

            // Then
            firstHandler.Verify(m => m.Handle(ping, It.IsAny<CancellationToken>()), Times.Exactly(1));
            secondHandler.Verify(m => m.Handle(ping, It.IsAny<CancellationToken>()), Times.Exactly(1));

            Assert.True(firstHandlerCalled < secondHandlerCalled);
        }


        public class DummyCommand : ICommand
        { }

        [Fact]
        public async Task Mediator_Send_CallsTheFirstCommandHandlerThatIsRegistered()
        {
            // Given
            var command = new DummyCommand();
            var commandHandler = new Mock<ICommandHandler<DummyCommand>>();
            object provider(Type type) => commandHandler.Object;
            var mediator = new Mediator(provider);

            // When
            await mediator.Send(command);

            // Then
            commandHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        public class PingQuery : IQuery<string>
        { }

        [Fact]
        public async Task Mediator_Send_CallsTheCorrectQueryHandlerAndReturnsItsResponse()
        {
            // Given
            var query = new PingQuery ();
            var queryHandler = new Mock<IQueryHandler<PingQuery, string>>();
            object provider(Type type) => queryHandler.Object;
            var mediator = new Mediator(provider);

            queryHandler
                .Setup(m => m.Handle(query, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult("Pong"));

            // When
            var result = await mediator.Send(query);

            // Then
            Assert.Equal("Pong", result);
            queryHandler.Verify(m => m.Handle(query, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
