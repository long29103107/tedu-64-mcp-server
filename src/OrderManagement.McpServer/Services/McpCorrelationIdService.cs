using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.McpServer.Services;

public sealed class McpCorrelationIdService : ICorrelationIdService
{
    public string CorrelationId { get; } = $"mcp-{Guid.NewGuid():N}";
}
