using Monai.Deploy.WorkloadManager.IntegrationTests.Models;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
{
    public class TaskDispatchMessage
    {
        public string? TestName { get; set; }

        public Workflow? Workflow { get; set; }
    }

    public static class TaskDispatchMessages
    {
        public static List<TaskDispatchMessage> TestData = new List<TaskDispatchMessage>()
        {
                new TaskDispatchMessage
                {
                    TestName = "WorkflowEvent_1",
                    Workflow = new Workflow
                    {
                        Description = "WorkflowEvent_1_Description",
                        Name = "WorkflowEvent_1_Name",
                        Version = "WorkflowEvent_1_Version",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "WorkflowEvent_1_AeTitle",
                            DataOrigins = new string[] {"WorkflowEvent_1_DataOrigins1", "WorkflowEvent_1_DataOrigins2" },
                            ExportDestinations = new string[] { "WorkflowEvent_1_ExportDestinations1", "WorkflowEvent_1_ExportDestinations2" }
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject
                            {
                                Id = "9dbf5138-df6f-4ca7-a6e8-7d410a1dab8e",
                                Description = "Task_Object_1",
                                Type = "Task",
                                Args = "Task_args",
                                Ref = "10101",
                                TaskDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "Task_destination",
                                        Conditions = new Evaluator[]
                                        {
                                            new Evaluator
                                            {
                                                CorrelationId = "138b66f9-8538-4c34-bc9a-28c24950fa98",
                                                Dicom = new Models.ExecutionContext
                                                {
                                                    ExecutionId = "",
                                                    TaskId = "",
                                                    InputDir = "",
                                                    OutputDir = "",
                                                    Task = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                    StartTime = 123.4567m,
                                                    EndTime = 123.4567m,
                                                    ErrorMsg = null,
                                                    Status = null,
                                                    Result = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                },
                                                Executions = new Models.ExecutionContext
                                                {
                                                    ExecutionId = "",
                                                    TaskId = "",
                                                    InputDir = "",
                                                    OutputDir = "",
                                                    Task = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                    StartTime = 123.4567m,
                                                    EndTime = 123.4567m,
                                                    ErrorMsg = null,
                                                    Status = null,
                                                    Result = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                },
                                                Input = new Artifact
                                                {
                                                    Name = "Artifact_Name",
                                                    Value = "Artifact_Value"
                                                }
                                            }
                                        }
                                    }
                                },
                                ExportDestinations = new TaskDestination[]
                                {
                                    new TaskDestination
                                    {
                                        Name = "Export_Destinations",
                                        Conditions = new Evaluator[]
                                        {
                                            new Evaluator
                                            {
                                                CorrelationId = "138b66f9-8538-4c34-bc9a-28c24950fa98",
                                                Dicom = new Models.ExecutionContext
                                                {
                                                    ExecutionId = "",
                                                    TaskId = "",
                                                    InputDir = "",
                                                    OutputDir = "",
                                                    Task = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                    StartTime = 123.4567m,
                                                    EndTime = 123.4567m,
                                                    ErrorMsg = null,
                                                    Status = null,
                                                    Result = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                },
                                                Executions = new Models.ExecutionContext
                                                {
                                                    ExecutionId = "",
                                                    TaskId = "",
                                                    InputDir = "",
                                                    OutputDir = "",
                                                    Task = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                    StartTime = 123.4567m,
                                                    EndTime = 123.4567m,
                                                    ErrorMsg = null,
                                                    Status = null,
                                                    Result = new Dictionary<string, string>()
                                                    {
                                                        {"test_1", "test_2" }
                                                    },
                                                },
                                                Input = new Artifact
                                                {
                                                    Name = "Artifact_Name",
                                                    Value = "Artifact_Value"
                                                }
                                            }
                                        }
                                    }
                                },
                                Artifacts = new ArtifactMap
                                {
                                    Input = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "Artifact_Name",
                                            Value = "Artifact_Value"
                                        }
                                    },
                                    Output = new Artifact[]
                                    {
                                        new Artifact
                                        {
                                            Name = "Artifact_Name",
                                            Value = "Artifact_Value"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
        };
    }
}
