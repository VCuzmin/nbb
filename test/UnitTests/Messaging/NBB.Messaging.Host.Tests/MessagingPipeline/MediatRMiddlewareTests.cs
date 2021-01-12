using System;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host.MessagingPipeline;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{

    public class MediatRMiddlewareTests
    {
        [Fact]
        public async void Should_publishEventsToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<MessagingContextAccessor>(), null);
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var envelope = new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Publish<INotification>(sentMessage, default), Times.Once);
        }

        [Fact]
        public async void Should_sendCommandsToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<MessagingContextAccessor>(), null);
            var sentMessage = Mock.Of<IMockingCommandMessage>();
            var envelope = new MessagingEnvelope<IMockingCommandMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Send(sentMessage, default), Times.Once);
        }

        [Fact]
        public void Should_throwExceptionForUnhandledMessageType()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<MessagingContextAccessor>(), null);
            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task Next() => Task.CompletedTask;

            //Act
            Task Action() => mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            
            ((Func<Task>)Action).Should().Throw<ApplicationException>();
        }

        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

  
            var ctxAccessor = Mock.Of<MessagingContextAccessor>(c => c.MessagingContext == new MessagingContext(envelope, typeof(IMockingEventMessage), "", new MessageSerDesOptions()));
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, ctxAccessor, Mock.Of<IMessageSerDes>());
                    Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

        public interface IMockingEventMessage : IMessage, IEvent, INotification { }
        public interface IMockingCommandMessage : IMessage, ICommand, IRequest { }
        public interface IMockingQueryMessage : IMessage, IQuery<string>, IRequest<string> { }
    }
}
