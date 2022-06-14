﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Common;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class ConditionalParameterParserTests
    {
        private readonly Mock<ILogger<ConditionalParameterParser>>? _logger;

        public ConditionalParameterParserTests()
        {
            _logger = new Mock<ILogger<ConditionalParameterParser>>();
        }

        [Theory]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' AND {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        //[InlineData(false, "{{context.dicom.tags[('0010','0040')]}} == 'F' OR {{context.executions.body_part_identifier.result.body_part}} == 'leg'")]
        [InlineData(
            "{{ context.executions.task['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'Fred' }} == 'Bob' AND " +
            "{{ context.executions.task['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'fred' }} == 'lowercasefred' AND " +
            "{{ context.executions.task['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'Sandra' }} == 'YassQueen' OR " +
            "{{ context.executions.task['other task'].'Fred' }} >= '32' OR " +
            "{{ context.executions.task['other task'].'Sandra' }} == 'other YassQueen' OR " +
            "{{ context.executions.task['other task'].'Derick' }} == 'lordge'", true)]
        [InlineData("{{ context.executions.task['other task'].'Derick' }} == 'lordge'", true)]
        public async Task ConditionalParameterParser_WhenGivenCorrectString_ShouldEvaluate(string input, bool expectedResult)
        {
            //  {{context.executions.task['TaskID'].'Key'}}
            var testData = CreateTestData();
            var workflow = testData.First();

            var conditionalParameterParser = new ConditionalParameterParser(_logger.Object);
            var actualResult = conditionalParameterParser.TryParse(input, workflow);

            Assert.Equal(expectedResult, actualResult);
        }

        public List<WorkflowInstance> CreateTestData()
        {
            //  {{context.executions.task['TaskID'].'Key'}}
            //  {{context.executions.task['2dbd1af7-b699-4467-8e99-05a0c22422b4'].'Fred'}}

            return new List<WorkflowInstance>()
            {
                new WorkflowInstance()
                {
                    Id = Guid.NewGuid().ToString(),
                    AeTitle = "Multi_Req",
                    WorkflowId = "workflow1",
                    PayloadId = "workflow1payload1",
                    StartTime = DateTime.Now,
                    Status = Status.Created,
                    InputMetaData = new Dictionary<string, string>()
                    {
                        { "a", "b" }
                    },
                    Tasks = new List<TaskExecution>
                    {
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "2dbd1af7-b699-4467-8e99-05a0c22422b4",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Succeeded,
                            Metadata = new Dictionary<string, object>()
                            {
                                { "Fred", "Bob" },
                                { "fred", "lowercasefred" },
                                { "Sandra", "YassQueen" },
                            }
                        },
                        new TaskExecution()
                        {
                            ExecutionId = Guid.NewGuid().ToString(),
                            TaskId = "other task",
                            TaskType = "Multi_task",
                            Status = TaskExecutionStatus.Succeeded,
                            Metadata = new Dictionary<string, object>()
                            {
                                { "Fred", "55" },
                                { "fred", "other lowercasefred" },
                                { "Sandra", "other YassQueen" },
                                { "Derick", "lordge" },
                            }
                        }
                    }
                }
                #region Extra Test Data
                
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Multi_Req",
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Request_Workflow_Dispatched").WorkflowRevision.WorkflowId,
                //    PayloadId = Helper.GetWorkflowRequestByName("Multi_WF_Dispatched").WorkflowRequestMessage.PayloadId.ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = "7d7c8b83-6628-413c-9912-a89314e5e2d5",
                //            TaskType = "Multi_task",
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Task_Status_Update_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_9",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_5",
                //            Status = TaskExecutionStatus.Succeeded
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_10",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_6",
                //            Status = TaskExecutionStatus.Failed
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = "Task_Update_11",
                //    WorkflowId = Guid.NewGuid().ToString(),
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Guid.NewGuid().ToString(),
                //            TaskType = "Multi_task_7",
                //            Status = TaskExecutionStatus.Canceled
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_2").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_3").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Accepted
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        },
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_1").WorkflowRevision.Workflow.Tasks[1].Type,
                //            Status = TaskExecutionStatus.Succeeded
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Independent_Task_Workflow").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //},
                //new WorkflowInstance()
                //{
                //    Id = Guid.NewGuid().ToString(),
                //    AeTitle = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.InformaticsGateway.AeTitle,
                //    WorkflowId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.WorkflowId,
                //    PayloadId = Guid.NewGuid().ToString(),
                //    StartTime = DateTime.Now,
                //    Status = Status.Created,
                //    BucketId = "bucket_1",
                //    InputMetaData = new Dictionary<string, string>()
                //    {
                //        { "", "" }
                //    },
                //    Tasks = new List<TaskExecution>
                //    {
                //        new TaskExecution()
                //        {
                //            ExecutionId = Guid.NewGuid().ToString(),
                //            TaskId = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Id,
                //            TaskType = Helper.GetWorkflowByName("Multi_Task_Workflow_Invalid_Task_Destination").WorkflowRevision.Workflow.Tasks[0].Type,
                //            Status = TaskExecutionStatus.Dispatched
                //        }
                //    }
                //}
                #endregion
            };
        }

    }

}
