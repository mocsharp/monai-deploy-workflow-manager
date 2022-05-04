using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monai.Deploy.WorkloadManager.Contracts.Models
{
    public class Workflow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public Guid? WorkflowId { get; set; }

        public int Revision { get; set; }

        public WorkflowSpec WorkflowSpec { get; set; }
    }
}
