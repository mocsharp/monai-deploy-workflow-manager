using System;
using Newtonsoft.Json;
namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class Task
    {
        [JsonProperty(PropertyName = "execution_id")]
        public Guid ExecutionId { get; set; }

        [JsonProperty(PropertyName = "task_type")]
        public string TaskType { get; set; }

        [JsonProperty(PropertyName = "task_plugin_arguments")]
        public TaskPluginArguments TaskPluginArguments { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "input_artifacts")]
        public InputArtifacts InputArtifacts { get; set; }

        [JsonProperty(PropertyName = "output_directory")]
        public string OutputDirectory { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Object Metadata { get; set; }
    }
}
