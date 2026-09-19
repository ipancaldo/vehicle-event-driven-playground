using AutoAuctionPlayground.Application.CQRS.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AutoAuctionPlayground.Application.CQRS.Dispatchers
{
    public sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _sp;
        public CommandDispatcher(IServiceProvider sp) => _sp = sp;

        public Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
        {
            var handler = _sp.GetRequiredService<ICommandHandler<TCommand>>();
            return handler.Handle(command, cancellationToken);
        }

        public Task<TResult> Dispatch<TCommand, TResult>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand<TResult>
        {
            var handler = _sp.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return handler.Handle(command, cancellationToken);
        }
    }
}
