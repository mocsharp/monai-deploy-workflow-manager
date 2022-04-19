using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;
using Monai.Deploy.WorkloadManager.IntegrationTests.TestData;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TestStepDefinitions
    {
        public TestStepDefinitions(RabbitClientUtil rabbitClientUtil, MongoClientUtil mongoClientUtil, MinioClientUtil minioClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
            MongoClientUtil = mongoClientUtil;
            MinioClientUtil = minioClientUtil;
            Assertions = new Assertions(RabbitClientUtil, MongoClientUtil);
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

        private MongoClientUtil MongoClientUtil { get; set; }

        private MinioClientUtil MinioClientUtil { get; set; }

        private Assertions Assertions { get; set; }

        private string CorrelationId { get; set; }

        [When(@"I publish an Export Message Request (.*)")]
        public void WhenIPublishAnExportMessageRequest(string testName)
        {
            var workflowTestData = WorkflowRequests.TestData.FirstOrDefault(c => c.TestName.Contains(testName));

            if (workflowTestData != null)
            {
                CorrelationId = workflowTestData.ExportMessageRequest.CorrelationId;
                var message = JsonConvert.SerializeObject(workflowTestData.ExportMessageRequest);
                RabbitClientUtil.PublishMessage(message, TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
            }
            else
            {
                throw new Exception($"{testName} does not have any applicable test data, please check and try again!");
            }
        }

        [Then(@"I can see the event (.*)")]
        public void ThenICanSeeTheEvent(string testName)
        {
            Assertions.AssertExportMessageRequest(testName, CorrelationId);
        }

        [Given(@"I have a DAG in Mongo (.*)")]
        public void IHaveADagInMongo(string testName)
        {
            var dagTestData = DummyDagTestData.TestData;
            if (dagTestData.DummyDag != null)
            {
                MongoClientUtil.CreateDummyDagDocument(dagTestData.DummyDag);
            }
            else
            {
                throw new Exception($"{testName} does not have any applicable test data, please check and try again!");
            }
        }

        [Then(@"I can retrieve the DAG (.*)")]
        public void ThenICanRetrieveTheDAG(string testName)
        {
            // Assertions.AssertMongoDagDocument(testName);
        }

        [Given(@"I have a MinIO spun up")]
        public void GivenIhaveaMinIOspunup()
        {
            MinioClientUtil.ListBuckets();
        }

        [When(@"I add a file")]
        public void WhenIaddafile()
        {
            return;
        }

        [Then(@"I can see the file")]
        public void ThenIcanseethefile()
        {
            return;
        }

        [Then(@"I can retrieve the file")]
        public void ThenIcanretrievethefile()
        {
            return;
        }
    }
}
