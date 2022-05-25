using Minio;
using Minio.DataModel;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.TaskManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {

        private AsyncRetryPolicy RetryPolicy { get; set; }

        public MinioClientUtil()
        {
            Client = new MinioClient()
                .WithEndpoint(TestExecutionConfig.MinIOConfig.ConnectionString)
                .WithCredentials(TestExecutionConfig.MinIOConfig.User, TestExecutionConfig.MinIOConfig.Password)
                .Build();

            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        private MinioClient Client { get; set; }

        public async Task AddFileToStorage(string fileLocation, string bucketName, string objectName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await Client.MakeBucketAsync(
                        new MakeBucketArgs()
                            .WithBucket(bucketName)
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }

                try
                {
                    byte[] bs = File.ReadAllBytes(fileLocation);

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
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task<ObjectStat> GetFile(string bucketName, string objectName, string fileName)
        {
            var args = new GetObjectArgs()
                                         .WithBucket(bucketName)
                                         .WithObject(objectName)
                                         .WithFile(fileName);

            return await Client.GetObjectAsync(args);
        }

        public async Task DeleteBucket(string bucketName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                await RemoveObjects(bucketName);

                var args = new RemoveBucketArgs().WithBucket(bucketName);

                await Client.RemoveBucketAsync(args);
            });
        }

        public async Task RemoveObjects(string bucketName)
        {
            bool found = await Client.BucketExistsAsync(bucketName);

            if (found)
            {
                var args = new RemoveObjectArgs()
                                         .WithBucket(bucketName)
                                         .WithObject("test");

                await Client.RemoveObjectAsync(args);
            }
        }
    }
}
