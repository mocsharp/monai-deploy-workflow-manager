using Minio;
using Minio.DataModel;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        public MinioClientUtil()
        {
            Client = new MinioClient()
                                    .WithEndpoint("play.min.io")
                                    .WithCredentials("Q3AM3UQ867SPQQA43P2F",
                                             "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG")
                                    .Build();
        }

        private MinioClient Client { get; set; }


        public async Task<ListAllMyBucketsResult> ListBuckets()
        {
            return Client.ListBucketsAsync().Result;
        }
    }
}
