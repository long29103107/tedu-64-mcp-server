// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using OrderManagement.Application;
// using OrderManagement.Application.Common.Interfaces;
// using OrderManagement.Application.Contracts;
// using OrderManagement.Infrastructure;
// using OrderManagement.McpServer.Middlewares;
// using OrderManagement.McpServer.Services;
//
// var builder = WebApplication.CreateBuilder(args);
//
// builder.Logging.ClearProviders();
// builder.Configuration
//     .SetBasePath(AppContext.BaseDirectory)
//     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
//     .AddEnvironmentVariables();
//
//
// builder.Services.AddApplicationServices();
// builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
//
// builder.Services.AddHttpContextAccessor();
//
// builder.Services.AddScoped<ICorrelationIdService, McpCorrelationIdService>();
// builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
//
// builder.Services
//     .AddMcpServer()
//     .WithHttpTransport()
//     .WithStdioServerTransport()
//     .WithResourcesFromAssembly()
//     .WithPromptsFromAssembly()
//     .WithToolsFromAssembly();
//
// var app = builder.Build();
// app.UseMiddleware<McpErrorHandlingMiddleware>();
// app.MapMcp("/mcp");
//
// await app.RunAsync();