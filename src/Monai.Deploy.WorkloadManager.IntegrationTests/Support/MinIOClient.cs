using Minio;
using Minio.DataModel;

namespace Monai.Deploy.WorkloadManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        public MinioClientUtil()
        {
            Client = new MinioClient()
                .WithEndpoint("localhost:9000")
                .WithCredentials("minioadmin", "minioadmin")
                .Build();
        }

        private MinioClient Client { get; set; }


        public async Task<ListAllMyBucketsResult> ListBuckets()
        {
            return Client.ListBucketsAsync().Result;
        }

        public async Task CreateBucket(string bucketName)
        {
            try
            {
                await Client.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(bucketName)
                );
                Console.WriteLine($"Created bucket {bucketName}");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }
        }

        public async Task AddFile(string fileLocation, string bucketName, string objectName)
        {
            try
            {
                byte[] bs = File.ReadAllBytes(fileLocation);
                Console.WriteLine("Running example for API: PutObjectAsync");
                using (MemoryStream filestream = new MemoryStream(bs))
                {
                    FileInfo fileInfo = new FileInfo(fileLocation);
                    var metaData = new Dictionary<string, string>
                    {
                        { "Test-Metadata", "Test  Test" }
                    };
                    PutObjectArgs args = new PutObjectArgs()
                                                    .WithBucket(bucketName)
                                                    .WithObject(objectName)
                                                    .WithStreamData(filestream)
                                                    .WithObjectSize(filestream.Length)
                                                    .WithContentType("application/octet-stream")
                                                    .WithHeaders(metaData);
                    await Client.PutObjectAsync(args);
                }

                Console.WriteLine($"Uploaded object {objectName} to bucket {bucketName}");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }
        }

        public async Task<Minio.DataModel.ObjectStat> GetFile(string bucketName, string objectName, string fileName)
        {
            Console.WriteLine("Running example for API: GetObjectAsync");
            GetObjectArgs args = new GetObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(objectName)
                                            .WithFile(fileName);
            return await Client.GetObjectAsync(args);
        }
    }
}
