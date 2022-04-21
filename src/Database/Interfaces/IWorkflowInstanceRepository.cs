using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkloadManager.Contracts.Models;

namespace Monai.Deploy.WorkloadManager.Database.Interfaces;

public interface IWorkflowInstanceRepository
{
    Task<bool> CreateAsync(IList<WorkflowInstance> workflows);
}
