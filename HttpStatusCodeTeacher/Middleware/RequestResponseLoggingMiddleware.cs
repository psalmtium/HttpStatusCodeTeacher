using System.Diagnostics;
using System.Text;

namespace HttpStatusCodeTeacher.Middleware;

/// <summary>
/// Middleware to log all HTTP requests and responses
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Log request
        await LogRequestAsync(context.Request, requestId);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Use a memory stream to capture the response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware in the pipeline
            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context.Response, requestId, stopwatch.ElapsedMilliseconds);

            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpRequest request, string requestId)
    {
        try
        {
            request.EnableBuffering();

            var body = string.Empty;
            if (request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Request");
            logMessage.AppendLine($"Method: {request.Method}");
            logMessage.AppendLine($"Path: {request.Path}{request.QueryString}");
            logMessage.AppendLine($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"))}");

            if (!string.IsNullOrEmpty(body))
            {
                logMessage.AppendLine($"Body: {body}");
            }

            _logger.LogInformation(logMessage.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error logging request", requestId);
        }
    }

    private async Task LogResponseAsync(HttpResponse response, string requestId, long elapsedMs)
    {
        try
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Response");
            logMessage.AppendLine($"Status: {response.StatusCode}");
            logMessage.AppendLine($"Duration: {elapsedMs}ms");
            logMessage.AppendLine($"Content-Type: {response.ContentType}");

            // Only log body for non-binary responses
            if (response.ContentType?.Contains("application/json") == true ||
                response.ContentType?.Contains("text/") == true)
            {
                // Limit body length for logging
                var maxBodyLength = 10000;
                var truncatedBody = bodyText.Length > maxBodyLength
                    ? bodyText.Substring(0, maxBodyLength) + "... (truncated)"
                    : bodyText;

                logMessage.AppendLine($"Body: {truncatedBody}");
            }

            _logger.LogInformation(logMessage.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error logging response", requestId);
        }
    }
}

/// <summary>
/// Extension method to register the logging middleware
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
