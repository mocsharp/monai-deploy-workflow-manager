using k8s;
using Monai.Deploy.WorkflowManager.TaskManager.Argo;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public class TestKubernetesProvider : IKubernetesProvider
    {
        public IKubernetes CreateClient() => throw new NotImplementedException();
    }
}
