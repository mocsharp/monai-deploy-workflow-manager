using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.WorkloadManager.Common.Services.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Responses;
using Monai.Deploy.WorkloadManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkloadManager.Controllers;

/// <summary>
/// Workflows Controller
/// </summary>
public class WorkflowsController : Controller
{
    private readonly IWorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowsController"/> class.
    /// </summary>
    /// <param name="workflowService"></param>
    public WorkflowsController(IWorkflowService workflowService)
    {
        this._workflowService = workflowService;
    }

    /// <summary>
    /// Get a workflow by the ID
    /// </summary>
    /// <param name="id">The Workflow Id</param>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpPost]
    public async Task<IActionResult> GetAsync([FromRoute] Guid? id)
    {
        if (!id.HasValue || id.Value == Guid.Empty)
        {
            return this.BadRequest();
        }

        var workflow = await this._workflowService.GetAsync(id.Value);

        return this.Ok(workflow);
    }

    /// <summary>
    /// Create a workflow
    /// </summary>
    /// <param name="workflow">The Workflow.</param>
    /// <returns>The ID of the created Workflow.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Workflow workflow)
    {
        if (!workflow.IsValid(out var validationErrors))
        {
            return this.BadRequest();
        }

        var workflowId = await this._workflowService.CreateAsync(workflow);

        return this.StatusCode(StatusCodes.Status201Created, new CreateWorkflowResponse(workflowId));
    }
}
