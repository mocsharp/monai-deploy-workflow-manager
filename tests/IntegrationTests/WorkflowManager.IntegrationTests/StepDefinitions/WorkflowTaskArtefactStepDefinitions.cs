using System;
using TechTalk.SpecFlow;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.StepDefinitions
{
    [Binding]
    public class WorkflowTaskArtefactStepDefinitions
    {
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
    }
}
