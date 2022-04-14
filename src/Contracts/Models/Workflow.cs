using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class Workflow
    {
        [JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public Guid? Id { get; set; }

        [JsonIgnore]
        public Guid? Reference { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "informatics_gateway")]
        public InformaticsGateway InformaticsGateway { get; set; }

        public TaskObject[] Tasks { get; set; }
    }
}
