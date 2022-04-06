﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkloadManager.Configuration;

namespace Monai.Deploy.WorkloadManager.PayloadListener.Services
{
    public class PayloadListenerService : ListenerServiceBase
    {
        protected override int Concurrency { get; }
        public override string WorkflowRequestRoutingKey { get; }
        public override string ServiceName => "Payload Listner Service";

        public PayloadListenerService(
            ILogger<PayloadListenerService> logger,
            IOptions<WorkloadManagerOptions> configuration,
            IServiceScopeFactory serviceScopeFactory,
            IEventPayloadListenerService eventPayloadListenerService)
            : base(logger, configuration, serviceScopeFactory, eventPayloadListenerService)
        {
            WorkflowRequestRoutingKey = $"{configuration.Value.Messaging.Topics.ExportRequestPrefix}.{configuration.Value.Messaging.Topics.WorkflowRequest}";
            Concurrency = 2;
        }
    }
}
