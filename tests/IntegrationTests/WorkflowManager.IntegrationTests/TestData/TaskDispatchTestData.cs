using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.IntegrationTests.TestData
{
    public class TaskDispatchTestData
    {
        public string? Name { get; set; }

        public TaskDispatchEvent? TaskDispatchEvent { get; set; }
    }

    public static class TaskDispatchesTestData
    {
        public static List<TaskDispatchTestData> TestData = new List<TaskDispatchTestData>()
        {
            new TaskDispatchTestData()
            {
                Name = "Task-Dispatch-Event",
                TaskDispatchEvent = new TaskDispatchEvent()
                {
                    WorkflowInstanceId = "c6cea536-80a0-46ca-96dc-daf4683ba834",
                    TaskId = "c0bd5f8d-badc-4568-8a7f-9587c4217322",
                    ExecutionId = "4c358ee2-d37c-4a53-8ce7-d0700dde5c75",
                    PayloadId = "b176856b-ee85-4152-b6f2-676bea8e08d6",
                    CorrelationId = "84e3c72f-0d86-43dc-8a99-1062e667b651",
                    TaskPluginType = "19490a25-999e-46e8-b6fb-084d098f8aa6",
                    TaskPluginArguments = new Dictionary<string, string>(),
                    Inputs = new List<Messaging.Common.Storage>(),
                    Outputs = new List<Messaging.Common.Storage>(),
                    Status = TaskExecutionStatus.Dispatched,
                    IntermediateStorage = new Messaging.Common.Storage(),
                    Metadata = new Dictionary<string, object>()
                }
            },
        };
    }
}
