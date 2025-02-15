﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Common
{
    public static class EventMapper
    {
        public static TaskDispatchEvent ToTaskDispatchEvent(TaskExecution task, string workflowInstanceId, string correlationId, string payloadId, StorageServiceConfiguration configuration)
        {
            Guard.Against.Null(task, nameof(task));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.NullOrWhiteSpace(payloadId, nameof(payloadId));
            Guard.Against.Null(configuration, nameof(configuration));

            var inputs = new List<Messaging.Common.Storage>();

            if (task?.InputArtifacts != null)
            {
                foreach (var inArtifact in task.InputArtifacts)
                {
                    inputs.Add(new Messaging.Common.Storage
                    {
                        SecuredConnection = bool.Parse(configuration.Settings["securedConnection"]),
                        Endpoint = configuration.Settings["endpoint"],
                        Bucket = configuration.Settings["bucket"],
                        RelativeRootPath = inArtifact.Value,
                        Name = inArtifact.Key
                    });
                }
            }

            return new TaskDispatchEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                TaskId = task.TaskId,
                ExecutionId = task.ExecutionId.ToString(),
                CorrelationId = correlationId,
                Status = TaskExecutionStatus.Created,
                TaskPluginArguments = task.TaskPluginArguments,
                Inputs = inputs,
                TaskPluginType = task.TaskType,
                Metadata = task.Metadata,
                PayloadId = payloadId,
                IntermediateStorage = new Messaging.Common.Storage
                {
                    Bucket = configuration.Settings["bucket"],
                    RelativeRootPath = $"{task.OutputDirectory}/tmp",
                    Endpoint = configuration.Settings["endpoint"],
                    Name = task.TaskId,
                    SecuredConnection = bool.Parse(configuration.Settings["securedConnection"])
                }
            };
        }

        public static ExportRequestEvent ToExportRequestEvent(IList<string> dicomImages, string[] exportDestinations, string taskId, string workflowInstanceId, string correlationId)
        {
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(correlationId, nameof(correlationId));
            Guard.Against.NullOrEmpty(dicomImages, nameof(dicomImages));
            Guard.Against.NullOrEmpty(exportDestinations, nameof(exportDestinations));

            return new ExportRequestEvent
            {
                WorkflowInstanceId = workflowInstanceId,
                ExportTaskId = taskId,
                CorrelationId = correlationId,
                Files = dicomImages,
                Destinations = exportDestinations
            };
        }
    }
}
