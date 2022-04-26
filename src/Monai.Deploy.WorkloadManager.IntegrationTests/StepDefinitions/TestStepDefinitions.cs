using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;
using Monai.Deploy.WorkloadManager.IntegrationTests.TestData;
using Newtonsoft.Json;
using FluentAssertions;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TestStepDefinitions
    {
        public TestStepDefinitions()
        {
            MinioClientUtil = new MinioClientUtil();
            Assertions = new Assertions(null, null);
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
        public async void GivenIhaveaMinIOspunup()
        {
            return;
        }

        [When(@"I add a bucket")]
        public async void WhenIaddabucket()
        {
            await MinioClientUtil.CreateBucket("test-bucket");

        }

        [When(@"I add a file")]
        public async void WhenIaddafile()
        {
            await MinioClientUtil.AddFile("../../../TestData/DummyDag.cs", "test-bucket", "testfile.txt");
        }

        [Then(@"I can retrieve the file")]
        public async void ThenIcanretrievethefile()
        {
                var res = await MinioClientUtil.GetFile("test-bucket", "testfile.txt", "testfile.txt");
                res.ObjectName.Should().BeEquivalentTo("testfile.txt");
        }
    }
}
