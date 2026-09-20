namespace AutoAuctionPlayground.Application.Exceptions
{
    // Raised when the database rejects a write on a unique index. This is how race-prone rules
    // (one transaction per listing, one open auction per listing, VIN per company) surface when
    // two requests both passed the in-memory checks; the API maps it to 409 Conflict.
    public class UniqueConstraintViolationException(string constraintName)
        : Exception($"A database uniqueness rule was violated ({constraintName}).")
    {
        public string ConstraintName { get; } = constraintName;
    }
}
