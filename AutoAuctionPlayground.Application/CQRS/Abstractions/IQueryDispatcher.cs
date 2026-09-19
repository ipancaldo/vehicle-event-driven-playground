namespace AutoAuctionPlayground.Application.CQRS.Abstractions
{
    public interface IQueryDispatcher
    {
        Task<TResult> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery<TResult>;
    }
}
