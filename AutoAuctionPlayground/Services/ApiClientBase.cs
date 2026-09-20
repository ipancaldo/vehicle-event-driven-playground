using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace AutoAuctionPlayground.Web.Services
{
    public abstract class ApiClientBase(HttpClient httpClient, ApiActivityLog activityLog)
    {
        protected HttpClient Http { get; } = httpClient;

        protected async Task<ApiResult<T>> SendForResult<T>(
            HttpMethod method,
            string path,
            object? body = null,
            string? note = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var request = new HttpRequestMessage(method, path);
                if (body is not null)
                    request.Content = JsonContent.Create(body, options: JsonOptions);

                using var response = await Http.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var error = await ReadProblemDetail(response, cancellationToken);
                    Record(method, path, (int)response.StatusCode, stopwatch.ElapsedMilliseconds, error, note);
                    return ApiResult<T>.Fail((int)response.StatusCode, error);
                }

                var value = response.StatusCode == System.Net.HttpStatusCode.NoContent
                    ? default
                    : await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

                Record(method, path, (int)response.StatusCode, stopwatch.ElapsedMilliseconds, null, note);
                return ApiResult<T>.Ok((int)response.StatusCode, value!);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                Record(method, path, 0, stopwatch.ElapsedMilliseconds, exception.Message, note);
                return ApiResult<T>.Fail(0, exception.Message);
            }
        }

        protected async Task<ApiResult> Send(
            HttpMethod method,
            string path,
            object? body = null,
            string? note = null,
            CancellationToken cancellationToken = default)
        {
            var result = await SendForResult<object>(method, path, body, note, cancellationToken);
            return result.Success
                ? ApiResult.Ok(result.StatusCode)
                : ApiResult.Fail(result.StatusCode, result.Error);
        }

        private void Record(HttpMethod method, string path, int statusCode, long durationMs, string? error, string? note)
            => activityLog.Record(new ApiActivityEntry(
                DateTime.UtcNow, method.Method, path, statusCode, durationMs, error, note));

        // The API returns RFC 7807 ProblemDetails for every handled failure; "detail" carries the
        // domain message ("Only published listings can be sold; this one is Sold.").
        private static async Task<string?> ReadProblemDetail(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                if (document.RootElement.TryGetProperty("detail", out var detail))
                    return detail.GetString();
                if (document.RootElement.TryGetProperty("title", out var title))
                    return title.GetString();
            }
            catch (JsonException)
            {
                // Not a JSON problem document — fall through to the status line.
            }

            return response.ReasonPhrase;
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    }
}
