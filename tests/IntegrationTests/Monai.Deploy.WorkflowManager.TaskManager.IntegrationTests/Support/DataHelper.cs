// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests
{
    public class DataHelper
    {
        private RetryPolicy<List<WorkflowInstance>> RetryWorkflowInstances { get; set; }
        private RetryPolicy<List<TaskDispatchEvent>> RetryTaskDispatches { get; set; }
        private RabbitConsumer TaskDispatchConsumer { get; set; }
        public string PayloadId { get; private set; }

        public DataHelper()
        {
            RetryWorkflowInstances = Policy<List<WorkflowInstance>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
            RetryTaskDispatches = Policy<List<TaskDispatchEvent>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        public string GetPayloadId()
        {
            return PayloadId = Guid.NewGuid().ToString();
        }

        //public TaskDispatchEvent GetTaskDispatchEvent(string name)
        //{
        //}
    }
}
