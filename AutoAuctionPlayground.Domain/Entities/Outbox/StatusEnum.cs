namespace AutoAuctionPlayground.Domain.Entities.Outbox
{
    public enum StatusEnum
    {
        Pending,
        Processing,
        Published,
        Failed
    }
}
