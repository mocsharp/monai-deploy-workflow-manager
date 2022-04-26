using System;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Responses
{
    public class CreateWorkflowResponse
    {
        public CreateWorkflowResponse(Guid workflowId)
        {
            this.WorkflowId = workflowId;
        }

        [JsonProperty("workflow_id")]
        public Guid WorkflowId { get; set; }
    }
}
