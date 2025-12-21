using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BpmnWorkflow.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using BpmnWorkflow.Client.Services;
using BpmnWorkflow.Client.Handlers;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuration
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await http.GetFromJsonAsync<Dictionary<string, string>>("appsettings.json");
var apiBaseUrl = config?["ApiBaseUrl"] ?? "https://localhost:7225";

if (config != null)
{
    foreach (var kvp in config)
    {
        builder.Configuration[kvp.Key] = kvp.Value;
    }
}

// Auth Handlers
builder.Services.AddTransient<AuthenticationHeaderHandler>();

// Base HttpClient configured with API base URL and Auth Handler
builder.Services.AddHttpClient("BpmnWorkflow.API", client => 
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthenticationHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BpmnWorkflow.API"));

// Radzen services
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// Local storage & auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// Application services (API wrappers, BPMN interop)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IDmnService, DmnService>();
builder.Services.AddScoped<PowerPointExportService>();
builder.Services.AddScoped<IBpmnInteropService, BpmnInteropService>();
builder.Services.AddScoped<BpmnWorkflow.Client.Services.ThemeService>();

await builder.Build().RunAsync();
