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
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Workflow> _workflowCollection;

    public WorkflowRepository(
        IMongoClient client,
        IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings)
    {
        _client = client;
        var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
        _workflowCollection = mongoDatabase.GetCollection<Workflow>(bookStoreDatabaseSettings.Value.WorkflowCollectionName);
    }

    public async Task<IList<Workflow>> GetAsync()
    {
        var workflow = await _workflowCollection.Find(_ => true).ToListAsync();

        return workflow;
    }

    public async Task<Workflow> GetByReferenceAsync(Guid reference)
    {
        var workflow = await _workflowCollection.Find(x => x.Reference == reference).FirstOrDefaultAsync();

        return workflow;
    }

    public async Task<Workflow> GetByAeTitleAsync(string aeTitle)
    {
        var workflow = await _workflowCollection.Find(x => x.InformaticsGateway.AeTitle == aeTitle).FirstOrDefaultAsync();

        return workflow;
    }

    public async Task CreateAsync(Workflow workflow)
    {
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await _workflowCollection.InsertOneAsync(workflow);
            await session.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync();
        }
    }
}
