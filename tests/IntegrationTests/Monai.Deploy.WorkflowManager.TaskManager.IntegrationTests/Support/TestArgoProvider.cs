using Argo;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    internal class TestArgoProvider : IArgoProvider
    {
        public IArgoClient CreateClient(string baseUrl, string? apiToken) => throw new NotImplementedException();
    }
}
