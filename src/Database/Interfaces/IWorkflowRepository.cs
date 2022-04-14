using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.Database.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<IList<Workflow>> GetAsync();

        Task<Workflow> GetByReferenceAsync(Guid id);

        Task<Workflow> GetByAeTitleAsync(string aeTitle);

        Task CreateAsync(Workflow workflow);
    }
}
