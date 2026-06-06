using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using OrderManagement.Application.Orders.Queries;
using OrderManagement.Domain.Entities;

namespace OrderManagement.McpServer.Resources;

[McpServerResourceType]
public class OrderResource(IMediator mediator)
{
    private static readonly string[] AvailableStatuses =
        Enum.GetNames(typeof(OrderStatus));

    // --- Single order resource ---
    [McpServerResource(
        UriTemplate = "oms://orders/{orderId}",
        Name = "Order Detail",
        Title = "Details of a single order with actual fields from the DTO: id, customerId, customerEmail, status, totalAmount, currency, shippingAddress, createdAt, updatedAt, items. Valid statuses: Draft, Placed, Confirmed, Shipped, Delivered, Cancelled.",
        MimeType = "application/json"
    )]
    public async Task<string> GetOrder(Guid orderId)
    {
        // Reuse the existing Query from the Application Layer
        var query = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(query);

        if (order is null)
        {
            // Return JSON with an error — do not throw an exception
            // The AI needs to read the error message to decide what to do next
            return JsonSerializer.Serialize(new
            {
                error = "NOT_FOUND",
                message = $"Order with ID '{orderId}' does not exist in the system.",
                suggestion = "Use the resource oms://orders/list to view the list of valid orders."
            });
        }

        var response = new
        {
            data = order,
            meta = new
            {
                availableStatuses = AvailableStatuses,
                fields = new[]
                {
                    "id",
                    "customerId",
                    "customerEmail",
                    "status",
                    "totalAmount",
                    "currency",
                    "shippingAddress",
                    "createdAt",
                    "updatedAt",
                    "items"
                }
            }
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    [McpServerResource(
        UriTemplate = "oms://orders/list",
        Name = "Orders List",
        Title = "List of orders with filtering and pagination. Fields of each item: id, orderNumber, orderDate, customerName, totalAmount, currency, status, itemCount. Note: customerName currently contains customerEmail. Valid statuses: Draft, Placed, Confirmed, Shipped, Delivered, Cancelled.",
        MimeType = "application/json"
    )]
    public async Task<string> ListOrders(
        OrderStatus? status = null, // Filter by status; null means all orders
        int page = 1,               // Current page
        int pageSize = 20)          // Number of records per page
    {
        // Validate pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100); // Maximum 100 to prevent excessive requests from AI

        var query = new GetOrdersPagedQuery(page, pageSize, status);

        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return JsonSerializer.Serialize(new
            {
                error = "ERROR",
                message = result.Error.Description,
                code = result.Error.Code
            });
        }

        var pagedResult = result.Value;

        // Return pagination metadata together with the data
        // The AI needs totalCount to decide whether it should read more pages
        var response = new
        {
            data = pagedResult.Items,
            pagination = new
            {
                currentPage = pagedResult.Page,
                pageSize = pagedResult.PageSize,
                totalCount = pagedResult.TotalCount,
                totalPages = pagedResult.TotalPages,
                hasNextPage = pagedResult.HasNextPage,
                hasPreviousPage = pagedResult.HasPreviousPage
            },
            filter = new { status = status?.ToString() ?? "all" },
            meta = new
            {
                availableStatuses = AvailableStatuses,
                fields = new[]
                {
                    "id",
                    "orderNumber",
                    "orderDate",
                    "customerName",
                    "totalAmount",
                    "currency",
                    "status",
                    "itemCount"
                },
                fieldNotes = new
                {
                    customerName = "This field currently contains CustomerEmail from the underlying DTO/query projection.",
                    orderDate = "This field is mapped from CreatedAt."
                }
            }
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}