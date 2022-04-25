using Monai.Deploy.WorkloadManager.IntegrationTests.Models;
using Monai.Deploy.WorkloadManager.IntegrationTests.POCO;
using Minio;
using System;

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

        private static MinioClient Client { get; set; }


        public async Task ListBuckets()
        {
            try
            {
                // List buckets that have read access.
                var list = await Client.ListBucketsAsync();
                foreach (var bucket in list.Buckets)
                {
                    Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
                }
            }
            catch ( Exception )
            {
                Console.WriteLine("Error occurred: ");
            }
        }
    }
}
