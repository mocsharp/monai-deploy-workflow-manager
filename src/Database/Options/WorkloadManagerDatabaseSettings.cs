namespace Monai.Deploy.WorkloadManager.Database.Options;

public class WorkloadManagerDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string WorkflowCollectionName { get; set; } = null!;
    public string WorkflowInstanceCollectionName { get; set; } = null!;
}
