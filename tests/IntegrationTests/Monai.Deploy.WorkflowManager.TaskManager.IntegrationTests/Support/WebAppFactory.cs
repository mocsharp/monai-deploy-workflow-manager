// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.AspNetCore.Mvc.Testing;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Monai.Deploy.WorkflowManager.TaskManager.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public static class WebAppFactory
    {
        public static void SetupTaskManager()
        {
            var webApplicationFactory = new WebApplicationFactory<Program>();
            //var webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            //{
            //    builder.ConfigureServices(services =>
            //    {
            //        services.AddSingleton<IArgoProvider, TestArgoProvider>();
            //        services.AddSingleton<IKubernetesProvider, TestKubernetesProvider>();
            //    });
            //});

            _ = webApplicationFactory.CreateClient();
        }

        //public static async Task<HttpResponseMessage> GetConsumers()
        //{
        //    var httpClient = new HttpClient();

        //    var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(TestExecutionConfig.RabbitConfig.User + ":" + TestExecutionConfig.RabbitConfig.Password));

        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", svcCredentials);

        //    return await httpClient.GetAsync($"http://{TestExecutionConfig.RabbitConfig.Host}:{TestExecutionConfig.RabbitConfig.Port}/api/consumers");
        //}
    }
}
