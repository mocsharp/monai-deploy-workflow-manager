using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Minio;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        public MinioClientUtil()
        {
            var connectionString = $"\"{TestExecutionConfig.MinioConfig.Host}:{TestExecutionConfig.MinioConfig.Port}\"," +
                $"\"{TestExecutionConfig.MinioConfig.AccessKey}\"," +
                $"\"{TestExecutionConfig.MinioConfig.SecretKey}\"";
            Client = new MinioClient(connectionString).WithSSL();
        }

        private MinioClient Client { get; set; }


        public void ListBuckets()
        {
            var getListBucketsTask = Client.ListBucketsAsync();
        }
    }
}
