using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BpmnWorkflow.Infrastructure.Background
{
    public class CamundaWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CamundaWorker> _logger;
        private const string WorkerId = "dotnet-worker-01";
        private readonly List<string> _topics = new() { "send-email", "calculate-discount", "notify-user" };

        public CamundaWorker(IServiceProvider serviceProvider, ILogger<CamundaWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Camunda Worker Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // await PollAndExecuteTasks(stoppingToken); 
                    // Temporarily disabled to prevent log spam due to 404 errors
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while polling Camunda tasks");
                }

                // Poll interval
                await Task.Delay(5000, stoppingToken);
            }

            _logger.LogInformation("Camunda Worker Service stopping.");
        }

        private async Task PollAndExecuteTasks(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var camundaService = scope.ServiceProvider.GetRequiredService<ICamundaService>();

            var request = new FetchExternalTasksRequest
            {
                WorkerId = WorkerId,
                MaxTasks = 10,
                Topics = _topics.Select(t => new TopicSubscription
                {
                    TopicName = t,
                    LockDuration = 30000 // 30 seconds
                }).ToList()
            };

            var tasks = await camundaService.FetchAndLockExternalTasksAsync(request, stoppingToken);

            if (tasks.Any())
            {
                _logger.LogInformation("Fetched {Count} external tasks from Camunda", tasks.Count);

                foreach (var task in tasks)
                {
                    await ProcessTask(camundaService, task, stoppingToken);
                }
            }
        }

        private async Task ProcessTask(ICamundaService camundaService, ExternalTaskDto task, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing task {TaskId} for topic {Topic}", task.Id, task.TopicName);

            try
            {
                var resultVariables = new Dictionary<string, object>();

                // Simulated logic based on topic
                switch (task.TopicName)
                {
                    case "send-email":
                        await SimulateEmailSending(task);
                        resultVariables["emailStatus"] = "sent";
                        resultVariables["sentAt"] = DateTime.UtcNow;
                        break;

                    case "calculate-discount":
                        var discount = CalculateDiscount(task);
                        resultVariables["discountAmount"] = discount;
                        break;

                    case "notify-user":
                        _logger.LogInformation("Notifying user about completion of {ActivityId}", task.ActivityId);
                        break;
                }

                // Complete the task
                var completeRequest = new CompleteExternalTaskRequest
                {
                    WorkerId = WorkerId,
                    Variables = resultVariables.Any() ? resultVariables : null
                };

                var success = await camundaService.CompleteExternalTaskAsync(task.Id, completeRequest, stoppingToken);
                
                if (success)
                    _logger.LogInformation("Successfully completed task {TaskId}", task.Id);
                else
                    _logger.LogWarning("Failed to complete task {TaskId} in Camunda", task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process task {TaskId}", task.Id);
                await camundaService.ReportExternalTaskFailureAsync(task.Id, WorkerId, ex.Message, 3, stoppingToken);
            }
        }

        private async Task SimulateEmailSending(ExternalTaskDto task)
        {
            // Simulate network latency
            await Task.Delay(2000);
            _logger.LogInformation("Email sent for process {InstanceId}", task.ProcessInstanceId);
        }

        private double CalculateDiscount(ExternalTaskDto task)
        {
            // Simple logic: if 'orderAmount' exists, give 10%
            if (task.Variables != null && task.Variables.TryGetValue("orderAmount", out var amountObj))
            {
                if (double.TryParse(amountObj.ToString(), out var amount))
                {
                    return amount * 0.1;
                }
            }
            return 0;
        }
    }
}
