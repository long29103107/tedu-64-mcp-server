using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Application.Orders.Queries;

namespace OrderManagement.McpServer.OrderTools;

[McpServerToolType]
public class OrderTools(IMediator mediator)
{
    [McpServerTool(Name = "get_order")]
    [Description("Retrieve detailed information about an order by its ID. Returns the order ID, status, product list, total amount, customer name, and creation date.")]
    public async Task<OrderDto?> GetOrder(
        [Description("The order ID to retrieve (GUID format).")]
        Guid orderId)
    {
        var query = new GetOrderByIdQuery(orderId);

        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value!;
    }
}