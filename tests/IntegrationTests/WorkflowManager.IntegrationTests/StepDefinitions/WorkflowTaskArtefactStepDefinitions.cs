// SPDX-FileCopyrightText: � 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Reflection;
using BoDi;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowTaskArtefactStepDefinitions
    {

        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private MinioClientUtil MinioClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }

        public WorkflowTaskArtefactStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            MinioClient = objectContainer.Resolve<MinioClientUtil>();
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        [Given(@"I have a bucket in MinIO (.*)")]
        public async Task GivenIHaveABucketInMinIO(string name)
        {
            await MinioClient.CreateBucket(name);
            var pathname = Path.Combine(GetDirectory(), "DICOMs", "MR000000.dcm");
            await MinioClient.AddFileToStorage(pathname, name, DataHelper.GetPayloadId());
        }

        [Then(@"I can see a task dispatch event with a path to the DICOM bucket")]
        public void ThenICanSeeATaskDispatchEventWithAPathToTheDICOMBucket()
        {
            throw new PendingStepException();
        }

        [Then(@"The workflow instance fails")]
        public void ThenTheWorkflowInstanceFails()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a task update message (.*)")]
        public void WhenIPublishATaskUpdateMessage(string name)
        {
            throw new PendingStepException();
        }

        [Then(@"The workflow instance is updated with correct file path")]
        public void ThenTheWorkflowInstanceIsUpdatedWithCorrectFilePath()
        {
            throw new PendingStepException();
        }

        [Then(@"The task dispatch message is updated with correct file path")]
        public void ThenTheTaskDispatchMessageIsUpdatedWithCorrectFilePath()
        {
            throw new PendingStepException();
        }

        [Given(@"I have a workflow instance")]
        public void GivenIHaveAWorkflowInstance()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a task dispatch Message (.*)")]
        public void WhenIPublishATaskDispatchMessage()
        {
            throw new PendingStepException();
        }

        [When(@"I publish a workflow instance (.*)")]
        public void WhenIPublishAWorkflowInstance()
        {
            throw new PendingStepException();
        }

        private string GetDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        [Given(@"I have an artefact in the bucket (.*)")]
        public async Task GivenIHaveAnArtefactInTheBucketCalled(string name)
        {
            var pathname = Path.Combine(GetDirectory(), "DICOMs", "MR000000.dcm");
            await MinioClient.AddFileToStorage(pathname, name, DataHelper.GetPayloadId());
        }

        [When(@"I publish a task dispatch message (.*)")]
        public void WhenIPublishATaskDispatchMessage(string name)
        {
            var message = new JsonMessage<TaskDispatchEvent>(
                DataHelper.GetTaskDispatchTestData(name),
                "16988a78-87b5-4168-a5c3-2cfc2bab8e54",
                Guid.NewGuid().ToString(),
                string.Empty);

            WorkflowPublisher.PublishMessage(message.ToMessage());
        }

        [Then(@"I recieve a task update message")]
        public void ThenIRecieveATaskUpdateMessage()
        {
            return;
        }


        /*[AfterScenario]
        public async Task DeleteObjects()
        {
            await MinioClient.RemoveObjects(TestExecutionConfig.MinioConfig.Bucket, DataHelper.PayloadId);
        }*/

    }
}
