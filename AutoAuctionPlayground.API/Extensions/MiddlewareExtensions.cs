using AutoAuctionPlayground.API.Middleware;

namespace AutoAuctionPlayground.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
