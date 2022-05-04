using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.Common.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<Workflow> GetAsync(Guid id);

        Task<Guid> CreateAsync(Workflow workflow);
    }
}
