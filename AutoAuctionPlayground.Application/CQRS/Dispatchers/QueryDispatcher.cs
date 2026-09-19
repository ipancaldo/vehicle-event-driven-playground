using AutoAuctionPlayground.Application.CQRS.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AutoAuctionPlayground.Application.CQRS.Dispatchers
{
    public sealed class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _sp;
        public QueryDispatcher(IServiceProvider sp) => _sp = sp;

        public Task<TResult> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery<TResult>
        {
            var handler = _sp.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return handler.Handle(query, cancellationToken);
        }
    }
}
