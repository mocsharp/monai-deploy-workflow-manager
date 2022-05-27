// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.AspNetCore.Mvc.Testing;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.TaskManager.Runner;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.MinIo;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.RabbitMq;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public static class WebAppFactory
    {
        public static void SetupTaskManager()
        {
            var webApplicationFactory = new WebApplicationFactory<Program>();
            //var webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            //{
            //    builder.ConfigureServices((hostContext, services) =>
            //    {
            //        services.AddOptions<WorkflowManagerOptions>().Bind(hostContext.Configuration.GetSection("WorkflowManager"));
            //        services.AddOptions<StorageServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:storage"));
            //        services.AddOptions<MessageBrokerServiceConfiguration>().Bind(hostContext.Configuration.GetSection("WorkflowManager:messaging"));

            //        services.AddHttpClient();
            //        services.UseRabbitMq();
            //        services.AddSingleton<IStorageService, MinIoStorageService>();
            //        services.AddSingleton<IMessageBrokerPublisherService, RabbitMqMessagePublisherService>();
            //        services.AddSingleton<IMessageBrokerSubscriberService, RabbitMqMessageSubscriberService>();

            //        services.AddSingleton<TaskManager>();
            //        services.AddSingleton<IArgoProvider, ArgoProvider>();
            //        services.AddSingleton<IKubernetesProvider, KubernetesProvider>();

            //        services.AddHostedService<TaskManager>(p => p.GetRequiredService<TaskManager>());
            //    });
            //});

            _ = webApplicationFactory.CreateClient();

            //public static async Task<HttpResponseMessage> GetConsumers()
            //{
            //    var httpClient = new HttpClient();

            //    var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

            //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

            //    return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.Port}/api/consumers");
            //}
        }
    }
}
