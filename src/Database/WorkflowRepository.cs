using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Database.Options;
using MongoDB.Driver;

namespace Monai.Deploy.WorkloadManager.Database;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly IMongoCollection<Workflow> _workflowCollection;

    public WorkflowRepository(
        IMongoClient client,
        IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
        _workflowCollection = mongoDatabase.GetCollection<Workflow>(bookStoreDatabaseSettings.Value.WorkflowCollectionName);
    }

    public async Task<Workflow> GetByWorkflowIdAsync(Guid workflowId)
    {
        var workflow = await _workflowCollection
            .Find(x => x.WorkflowId == workflowId)
            .Sort(Builders<Workflow>.Sort.Descending("Revision"))
            .FirstOrDefaultAsync();

        return workflow;
    }

    public async Task<IList<Workflow>> GetByWorkflowsIdsAsync(IList<Guid> workflowIds)
    {
        var filterDef = new FilterDefinitionBuilder<Workflow>();
        var filter = filterDef.In(x => x.WorkflowId.Value, workflowIds);

        var workflows = await _workflowCollection
            .Find(filter).ToListAsync();

        return workflows;
    }

    public async Task<Workflow> GetByAeTitleAsync(string aeTitle)
    {
        var workflow = await _workflowCollection
            .Find(x => x.WorkflowSpec.InformaticsGateway.AeTitle == aeTitle)
            .Sort(Builders<Workflow>.Sort.Descending("Revision"))
            .FirstOrDefaultAsync();

        return workflow;
    }

    public async Task<Guid> CreateAsync(Workflow workflow)
    {
        if (!workflow.WorkflowId.HasValue)
        {
            workflow.WorkflowId = Guid.NewGuid();
            await _workflowCollection.InsertOneAsync(workflow);
        }
        else
        {
            var existingWorkflow = await GetByWorkflowIdAsync(workflow.WorkflowId.Value);
            workflow.WorkflowId = existingWorkflow.WorkflowId;
            workflow.Revision = existingWorkflow.Revision++;
            await _workflowCollection.InsertOneAsync(workflow);
        }

        return workflow.WorkflowId!.Value;
    }
}
