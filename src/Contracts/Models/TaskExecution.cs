﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Events;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskExecution
    {
        [JsonProperty(PropertyName = "execution_id")]
        public string ExecutionId { get; set; }

        [JsonProperty(PropertyName = "task_type")]
        public string TaskType { get; set; }

        [JsonProperty(PropertyName = "task_start_time")]
        public DateTime? TaskStartTime { get; set; }

        [JsonProperty(PropertyName = "execution_stats")]
        public Dictionary<string, object> ExecutionStats { get; set; }

        [JsonProperty(PropertyName = "task_plugin_arguments")]
        public Dictionary<string, string> TaskPluginArguments { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "previous_task_id")]
        public string PreviousTaskId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public TaskExecutionStatus Status { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public FailureReason Reason { get; set; }

        [JsonProperty(PropertyName = "input_artifacts")]
        public Dictionary<string, string> InputArtifacts { get; set; }

        [JsonProperty(PropertyName = "output_artifacts")]
        public Dictionary<string, string> OutputArtifacts { get; set; }

        [JsonProperty(PropertyName = "output_directory")]
        public string OutputDirectory { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty(PropertyName = "input_parameters")]
        public Dictionary<string, object> InputParameters { get; set; }
    }
}
