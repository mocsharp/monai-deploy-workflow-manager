// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using BoDi;
using FluentAssertions;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.WorkflowManager.IntegrationTests.Models;
using Monai.Deploy.WorkflowManager.IntegrationTests.Support;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowTaskArtefactStepDefinitions
    {

        private RabbitPublisher WorkflowPublisher { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        private MongoClientUtil MongoClient { get; set; }
        private Assertions Assertions { get; set; }
        private DataHelper DataHelper { get; set; }

        public WorkflowTaskArtefactStepDefinitions(ObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            WorkflowPublisher = objectContainer.Resolve<RabbitPublisher>("WorkflowPublisher");
            TaskDispatchConsumer = objectContainer.Resolve<RabbitConsumer>("TaskDispatchConsumer");
            MongoClient = objectContainer.Resolve<MongoClientUtil>();
            Assertions = new Assertions();
            DataHelper = objectContainer.Resolve<DataHelper>();
        }

        [Given(@"I have a bucket in MinIO (.*)")]
        public void GivenIHaveABucketInMinIO(string name)
        {
            throw new PendingStepException();
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
        public void WhenIPublishATaskUpdateMessage()
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

        [Then(@"I can see (.*) Instance is created with (.*) artefacts")]
        public void ThenICanSeeIsCreatedWithArtefacts(int count, string name)
        {
            var workflowInstances = DataHelper.GetWorkflowInstances(count, DataHelper.WorkflowRequestMessage.PayloadId.ToString());

            if (workflowInstances != null)
            {
                foreach (var workflowInstance in workflowInstances)
                {
                    var workflow = DataHelper.WorkflowRevisions.FirstOrDefault(x => x.WorkflowId.Equals(workflowInstance.WorkflowId));

                    if (workflow != null)
                    {
                        Assertions.AssertWorkflowInstanceMatchesExpectedWorkflow(workflowInstance, workflow, DataHelper.WorkflowRequestMessage);
                    }
                    else
                    {
                        throw new Exception($"Workflow not found for workflowId {workflowInstance.WorkflowId}");
                    }
                }
            }
        }

    }
}
