using System.Text.Json;
using ModelContextProtocol.Server;

namespace OrderManagement.McpServer.Resources;

[McpServerResourceType]
public class SystemInfoResource
{
    [McpServerResource(
        UriTemplate = "oms://system/info",
        Name = "OMS System Information",
        Title = "OMS system metadata: version, configuration, and order status definitions based on the domain model. " +
                "Read this resource before performing any order-related actions.",
        MimeType = "application/json"
    )]
    
    public string GetSystemInfo()
    {
        var info = new
        {
            system = new
            {
                name = "OMS - Order Management System",
                version = "1.0.0",
                description = "Order management system built using Clean Architecture"
            },
            orderStatuses = new[]
            {
                new
                {
                    code = "Draft",
                    label = "Draft",
                    description = "Order is newly created or being edited and has not been officially placed yet"
                },
                new
                {
                    code = "Placed",
                    label = "Placed",
                    description = "Order has been placed from the Draft status"
                },
                new
                {
                    code = "Confirmed",
                    label = "Confirmed",
                    description = "Order has been confirmed and is ready for shipment processing"
                },
                new
                {
                    code = "Shipped",
                    label = "Shipped",
                    description = "Order has been handed over to the shipping carrier"
                },
                new
                {
                    code = "Delivered",
                    label = "Delivered",
                    description = "Order has been successfully delivered to the customer"
                },
                new
                {
                    code = "Cancelled",
                    label = "Cancelled",
                    description = "Order has been cancelled and cannot be restored"
                }
            },
            businessRules = new
            {
                canCancelStatuses = new[] { "Draft", "Placed", "Confirmed" },
                nonModifiableStatuses = new[] { "Shipped", "Cancelled" },
                maxItemsPerOrder = 50,
                currency = "VND"
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return JsonSerializer.Serialize(info, options);
    }
}