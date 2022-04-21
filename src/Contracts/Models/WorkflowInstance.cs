using Newtonsoft.Json;
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class WorkflowInstance
    {
        [JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ae_title")]
        public string AeTitle { get; set; }

        [JsonProperty(PropertyName = "workflow_id")]
        public Guid WorkflowId { get; set; }

        [JsonProperty(PropertyName = "workflow_revision_id")]
        public int WorkflowRevisionId { get; set; }

        [JsonProperty(PropertyName = "payload_id")]
        public Guid PayloadId { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "bucket_id")]
        public string BucketId { get; set; }

        [JsonProperty(PropertyName = "input_metadata")]
        public InputMataData InputMataData { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public Task[] Tasks { get; set; }
    }
}
