using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Monai.Deploy.WorkloadManager.IntegrationTests.Support;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class TestStepDefinitions
    {
        public TestStepDefinitions(RabbitClientUtil rabbitClientUtil)
        {
            RabbitClientUtil = rabbitClientUtil;
            Assertions = new Assertions(RabbitClientUtil);
            // MongoClientUtil = mongoClientUtil;
        }

        private RabbitClientUtil RabbitClientUtil { get; set; }

        //private MongoClientUtil MongoClientUtil { get; set; }

        private Assertions Assertions { get; set; }

        [Given(@"I have a Rabbit connection")]
        public void GivenIHaveARabbitConnection()
        {
            RabbitClientUtil.CreateQueue(TestExecutionConfig.RabbitConfig.WorkflowRequestQueue);
        }

        [When(@"I publish an Export Message Request (.*)")]
        public void WhenIPublishAnExportMessageRequest(string testName)
        {
            var workflowTestData = TestData.WorkflowRequests.TestData.FirstOrDefault(c => c.TestName.Contains(testName));
            if (workflowTestData != null)
            {
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
            Assertions.AssertExportMessageRequest(testName);
        }

        [Given(@"I have a Mongo connection")]
        public void GivenIHaveAMongoConnection()
        {
        }

        [When(@"I save a DAG")]
        public void WhenISaveADAG()
        {
        }

        [Then(@"I can retrieve the DAG")]
        public void ThenICanRetrieveTheDAG()
        {
        }
    }
}
