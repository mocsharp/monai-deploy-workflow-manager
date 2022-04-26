using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.WorkloadManager.Common.Services.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.Contracts.Responses;
using Monai.Deploy.WorkloadManager.Controllers;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Monai.Deploy.WorkloadManager.Test.Controllers
{
    public class WorkflowsControllerTests
    {
        private readonly Mock<IWorkflowService> _mockWorkflowService;

        public WorkflowsControllerTests()
        {
            _mockWorkflowService = new();
        }

        [Fact]
        public async Task GetAsync_ValidRequest_ShouldReturnWorkflow()
        {
            var mockWorkflow = new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                Revision = 1,
                WorkflowSpec = new()
                {
                    Description = "Workflow Description",
                    Name = "Workflow 1",
                    Version = "1",
                    InformaticsGateway = new()
                    {
                        AeTitle = "The AeTitle",
                    },
                    Tasks = new TaskObject[]
                    {
                        new()
                    }
                }
            };

            var mockWorkflowId = Guid.NewGuid();

            _mockWorkflowService
                .Setup(x => x.GetAsync(It.IsAny<Guid>()))
                .ReturnsAsync(mockWorkflow);

            var sut = BuildSut();

            var response = await sut.GetAsync(mockWorkflowId);

            response.Should().BeOfType<OkObjectResult>();

            var resultAsOkObjectResult = response as OkObjectResult;
            resultAsOkObjectResult!.Value.Should().BeOfType<Workflow>();
            resultAsOkObjectResult.Value.Should().BeEquivalentTo(mockWorkflow);

            _mockWorkflowService.Verify(x => x.GetAsync(It.Is<Guid>(y => y == mockWorkflowId)), Times.Once);
            _mockWorkflowService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAsync_WorkflowIdIsNullOrEmpty_ShouldReturnBadRequest()
        {
            var mockWorkflow = new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                Revision = 1,
                WorkflowSpec = new()
                {
                    Description = "Workflow Description",
                    Name = "Workflow 1",
                    Version = "1",
                    InformaticsGateway = new()
                    {
                        AeTitle = "The AeTitle",
                    },
                    Tasks = new TaskObject[]
                    {
                        new()
                    }
                }
            };

            _mockWorkflowService
                .Setup(x => x.GetAsync(It.IsAny<Guid>()))
                .ReturnsAsync(mockWorkflow);

            var sut = BuildSut();

            var response = await sut.GetAsync(null);

            response.Should().BeOfType<BadRequestResult>();

            _mockWorkflowService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ShouldReturn201Created()
        {
            var mockRequest = new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                Revision = 1,
                WorkflowSpec = new()
                {
                    Description = "Workflow Description",
                    Name = "Workflow 1",
                    Version = "1",
                    InformaticsGateway = new()
                    {
                        AeTitle = "The AeTitle",
                        DataOrigins = new[] { "test 1", "test 2" },
                        ExportDestinations = new[] { "test 1", "test 2" }
                    },
                    Tasks = new TaskObject[]
                    {
                        new()
                        {
                            Id = "123",
                            Description = "Description",
                            Type = "type",
                            Args = new(),
                            Ref = "ref"
                        }
                    }
                }
            };

            var workflowId = Guid.NewGuid();
            var mockResponse = new CreateWorkflowResponse(workflowId);

            _mockWorkflowService
                .Setup(x => x.CreateAsync(It.IsAny<Workflow>()))
                .ReturnsAsync(workflowId);

            var sut = BuildSut();

            var response = await sut.CreateAsync(mockRequest);

            response.Should().BeOfType<ObjectResult>();

            var resultAsOkObjectResult = response as ObjectResult;
            resultAsOkObjectResult!.Value.Should().BeOfType<CreateWorkflowResponse>();
            resultAsOkObjectResult.Value.Should().BeEquivalentTo(mockResponse);

            _mockWorkflowService.Verify(x => x.CreateAsync(It.IsAny<Workflow>()), Times.Once);
            _mockWorkflowService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateAsync_InvalidRequest_ShouldReturnBadRequest()
        {
            var mockRequest = new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                Revision = 1,
                WorkflowSpec = new()
                {
                    Name = "", // Invalid name
                    InformaticsGateway = new InformaticsGateway(),
                    Tasks = new TaskObject[] { }
                },
            };

            var sut = BuildSut();

            var response = await sut.CreateAsync(mockRequest);

            response.Should().BeOfType<BadRequestResult>();

            _mockWorkflowService.VerifyNoOtherCalls();
        }

        private WorkflowsController BuildSut() => new(_mockWorkflowService.Object);
    }
}
