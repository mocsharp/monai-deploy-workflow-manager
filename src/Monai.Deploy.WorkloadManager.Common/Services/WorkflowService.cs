using Monai.Deploy.WorkloadManager.Common.Services.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Database.Interfaces;

namespace Monai.Deploy.WorkloadManager.Common.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowService(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        public async Task<Workflow> GetAsync(Guid id)
        {
            var workflow = await _workflowRepository.GetByWorkflowIdAsync(id);

            return workflow;
        }

        public async Task<Guid> CreateAsync(Workflow workflow)
        {
            var createdWorkflow = await _workflowRepository.CreateAsync(workflow);

            return createdWorkflow;
        }
    }
}
