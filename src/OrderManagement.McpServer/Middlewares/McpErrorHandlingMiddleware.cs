using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrderManagement.McpServer.Middlewares;

public class McpErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<McpErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Log đầy đủ để dev debug
            logger.LogError(ex,
                "Unhandled exception in MCP Server. Path: {Path}",
                context.Request.Path);


            // Trả về MCP-compatible error response
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";


            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = -32603,  // JSON-RPC Internal Error
                    message = "Internal server error. Please try again later."
                    // KHÔNG expose ex.Message — có thể chứa thông tin nhạy cảm
                }
            });
        }
    }
}