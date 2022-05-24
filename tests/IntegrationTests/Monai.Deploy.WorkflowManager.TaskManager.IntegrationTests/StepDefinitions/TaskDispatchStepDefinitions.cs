using System.Reflection;
using BoDi;
using Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TaskDispatchStepDefinitions
    {
        private RetryPolicy RetryPolicy { get; set; }
        private DataHelper DataHelper { get; set; }
        private Assertions Assertions { get; set; }
        private RabbitPublisher TaskDispatchPublisher { get; set; }
        private MinioClientUtil MinioClient { get; set; }

        public TaskDispatchStepDefinitions(ObjectContainer objectContainer)
        {
            TaskDispatchPublisher = objectContainer.Resolve<RabbitPublisher>("TaskDispatchPublisher");
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
            DataHelper = objectContainer.Resolve<DataHelper>();
            RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            Assertions = new Assertions();
        }

        [Given(@"A study is uploaded to the storage service")]
        public async Task AStudyIsUploadedToTheStorageService()
        {
            await MinioClient.AddFileToStorage("C:\\development\\AI\\AIDE\\MONAI\\monai-deploy-workflow-manager\\tests\\IntegrationTests\\Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests\\bin\\Debug\\net6.0\\DICOMs\\MR000000.dcm", DataHelper.GetPayloadId(), "test");
        }

        [When(@"A Task Dispatch event is published")]
        public void ATaskDispatchEventIsPublished()
        {
            Console.Write("Test");
        }

        [Then(@"I can see the event is consumed")]
        public void ThenICanSeeTheEventIsConsumed()
        {
            Console.Write("Test");
        }

        private string GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
        }

        [AfterScenario]
        public async Task DeleteBucket()
        {
            await MinioClient.DeleteBucket(DataHelper.PayloadId);
        }

    }
}
