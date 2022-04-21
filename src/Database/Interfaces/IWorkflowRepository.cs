using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.Database.Interfaces;

public interface IWorkflowRepository
{
    Task<Workflow> GetByWorkflowIdAsync(Guid id);

    Task<IList<Workflow>> GetByWorkflowsIdsAsync(IList<Guid> ids);

    Task<Workflow> GetByAeTitleAsync(string aeTitle);
}
