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
            Client = new MinioClient(TestExecutionConfig.MinioConfig.Endpoint, TestExecutionConfig.MinioConfig.AccessKey, TestExecutionConfig.MinioConfig.AccessToken);

            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        private MinioClient Client { get; set; }

        public async Task CreateBucket(string bucketName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await Client.MakeBucketAsync(bucketName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task AddFileToStorage(string fileLocation, string bucketName, string objectName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    byte[] bs = File.ReadAllBytes(fileLocation);

                    using (var stream = new MemoryStream(bs))
                    {
                        var fileInfo = new FileInfo(fileLocation);
                        var metaData = new Dictionary<string, string>
                            {
                                { "Test-Metadata", "Test  Test" }
                            };
                        await Client.PutObjectAsync(bucketName, objectName, stream, stream.Length, "application/octet-stream", metaData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                }
            });
        }

        //public async Task<ObjectStat> GetFile(string bucketName, string objectName, string fileName)
        //{
        //    var args = new GetObjectArgs()
        //                                 .WithBucket(bucketName)
        //                                 .WithObject(objectName)
        //                                 .WithFile(fileName);

        //    return await Client.GetObjectAsync(args);
        //}

        //public async Task DeleteBucket(string bucketName)
        //{
        //    bool found = await Client.BucketExistsAsync(bucketName);

        //    if (found)
        //    {
        //        await RetryPolicy.ExecuteAsync(async () =>
        //        {
        //            var args = new RemoveBucketArgs().WithBucket(bucketName);

        //            await Client.RemoveBucketAsync(args);
        //        });
        //    }
        //}

        //public async Task RemoveObjects(string bucketName, string objectName)
        //{
        //    bool found = await Client.BucketExistsAsync(bucketName);

        //    if (found)
        //    {
        //        var args = new RemoveObjectArgs()
        //                                 .WithBucket(bucketName)
        //                                 .WithObject(objectName);

        //        await Client.RemoveObjectAsync(args);
        //    }
        //}
    }
}
