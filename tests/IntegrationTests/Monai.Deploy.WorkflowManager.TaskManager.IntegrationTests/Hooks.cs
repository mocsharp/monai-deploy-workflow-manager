// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using Microsoft.Extensions.Configuration;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    /// <summary>
    /// Hooks class for setting up the integration tests.
    /// </summary>
    [Binding]
    public class Hooks
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hooks"/> class.
        /// </summary>
        /// <param name="objectContainer"></param>
        public Hooks(IObjectContainer objectContainer)
        {
            ObjectContainer = objectContainer;
        }

        private static RabbitPublisher? TaskDispatchPublisher { get; set; }
        private static RabbitPublisher? TaskCallbackPublisher { get; set; }
        private static RabbitConsumer? TaskUpdateConsumer { get; set; }
        private static MinioClientUtil MinioClient { get; set; }
        private IObjectContainer ObjectContainer { get; set; }

        /// <summary>
        /// Runs before all tests to create static implementions of Rabbit and Mongo clients as well as starting the WorkflowManager using WebApplicationFactory.
        /// </summary>
        [BeforeTestRun(Order = 0)]
        public static void Init()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();

            TestExecutionConfig.RabbitConfig.Host = "localhost";
            TestExecutionConfig.RabbitConfig.Port = 15672;
            TestExecutionConfig.RabbitConfig.User = "admin";
            TestExecutionConfig.RabbitConfig.Password = "admin";
            TestExecutionConfig.RabbitConfig.VirtualHost = "monaideploy";
            TestExecutionConfig.RabbitConfig.Exchange = "monaideploy";
            TestExecutionConfig.RabbitConfig.TaskDispatchQueue = "md.tasks.dispatch";
            TestExecutionConfig.RabbitConfig.TaskCallbackQueue = "md.tasks.callback";
            TestExecutionConfig.RabbitConfig.TaskUpdateQueue = "md.tasks.update";

            TestExecutionConfig.MinIOConfig.ConnectionString = "localhost:9000";
            TestExecutionConfig.MinIOConfig.User = "minioadmin";
            TestExecutionConfig.MinIOConfig.Password = "minioadmin";
            TestExecutionConfig.MinIOConfig.BucketName = "monaideploy";

            TaskDispatchPublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskDispatchQueue);
            TaskCallbackPublisher = new RabbitPublisher(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskCallbackQueue);
            TaskUpdateConsumer = new RabbitConsumer(RabbitConnectionFactory.GetConnectionFactory(), TestExecutionConfig.RabbitConfig.Exchange, TestExecutionConfig.RabbitConfig.TaskUpdateQueue);
            MinioClient = new MinioClientUtil();
            WebAppFactory.SetupTaskManager();
        }

        [BeforeTestRun(Order = 2)]
        public async static Task SetupBucket()
        {
            await MinioClient.CreateBucket(TestExecutionConfig.MinIOConfig.BucketName);
        }

        /// <summary>
        /// Runs before all tests to check that the WorkflowManager consumer is started.
        /// </summary>
        /// <returns>Error if the WorkflowManager consumer is not started.</returns>
        /// <exception cref="Exception"></exception>
        //[BeforeTestRun(Order = 1)]
        //public static async Task CheckWorkflowConsumerStarted()
        //{
        //    var response = await WebAppFactory.GetConsumers();
        //    var content = response.Content.ReadAsStringAsync().Result;

        //    for (var i = 1; i <= 10; i++)
        //    {
        //        if (string.IsNullOrEmpty(content) || content == "[]")
        //        {
        //            Debug.Write($"Workflow consumer not started. Recheck times {i}");
        //            response = await WebAppFactory.GetConsumers();
        //            content = response.Content.ReadAsStringAsync().Result;
        //        }
        //        else
        //        {
        //            Debug.Write("Consumer started. Integration tests will begin!");
        //            break;
        //        }

        //        if (i == 10)
        //        {
        //            throw new Exception("Workflow Mangaer Consumer not started! Integration tests will not continue");
        //        }

        //        Thread.Sleep(1000);
        //    }
        //}

        /// <summary>
        /// Adds Rabbit and Mongo clients to Specflow IoC container for test scenario being executed.
        /// </summary>
        [BeforeScenario]
        public void SetUp()
        {
            ObjectContainer.RegisterInstanceAs(TaskDispatchPublisher, "TaskDispatchPublisher");
            ObjectContainer.RegisterInstanceAs(TaskCallbackPublisher, "TaskCallbackPublisher");
            ObjectContainer.RegisterInstanceAs(TaskUpdateConsumer, "TaskUpdateConsumer");
            ObjectContainer.RegisterInstanceAs(MinioClient);
            var dataHelper = new DataHelper();
            ObjectContainer.RegisterInstanceAs(dataHelper);
        }

        [BeforeTestRun(Order = 1)]
        [AfterTestRun(Order = 0)]
        public async static Task ClearTestData()
        {
            await MinioClient.DeleteBucket(TestExecutionConfig.MinIOConfig.BucketName);
        }

        /// <summary>
        /// Runs after all tests to closes Rabbit connections.
        /// </summary>
        [AfterTestRun(Order = 1)]
        public static void TearDownRabbit()
        {
            if (TaskDispatchPublisher != null)
            {
                TaskDispatchPublisher.CloseConnection();
            }

            if (TaskUpdateConsumer != null)
            {
                TaskUpdateConsumer.CloseConnection();
            }
        }
    }
}
