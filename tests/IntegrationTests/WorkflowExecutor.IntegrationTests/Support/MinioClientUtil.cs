﻿using Minio;
using Monai.Deploy.WorkflowManager.IntegrationTests.POCO;
using Polly;
using Polly.Retry;
using System.Reactive.Linq;


namespace Monai.Deploy.WorkflowManager.IntegrationTests.Support
{
    public class MinioClientUtil
    {
        private AsyncRetryPolicy RetryPolicy { get; set; }
        private MinioClient Client { get; set; }

        public MinioClientUtil()
        {
            Client = new MinioClient()
                .WithEndpoint(TestExecutionConfig.MinioConfig.Endpoint)
                .WithCredentials(
                    TestExecutionConfig.MinioConfig.AccessKey,
                    TestExecutionConfig.MinioConfig.AccessToken
                ).Build();

            RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(500));
        }

        public async Task CreateBucket(string bucketName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    if (await Client.BucketExistsAsync(bucketName))
                    {
                        try
                        {
                            var listOfKeys = new List<string>();
                            var count = 0;
                            var listArgs = new ListObjectsArgs()
                                .WithBucket(bucketName)
                                .WithPrefix("")
                                .WithRecursive(true);

                            var objs = await Client.ListObjectsAsync(listArgs).ToList();
                            foreach (var obj in objs)
                            {
                                await Client.RemoveObjectAsync(bucketName, obj.Key);
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    else
                    {
                        await Client.MakeBucketAsync(bucketName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Bucket]  Exception: {e}");
                    if (e.Message != "MinIO API responded with message=Your previous request to create the named bucket succeeded and you already own it.")
                    {
                        throw e;
                    }
                }
            });
        }

        public async Task AddFileToStorage(string fileLocation, string bucketName, string objectName)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    FileAttributes fileAttributes = File.GetAttributes(fileLocation);
                    if (fileAttributes.HasFlag(FileAttributes.Directory))
                    {
                        var files = Directory.GetFiles($"{fileLocation}", "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            var relativePath = $"{objectName}/dcm/{Path.GetRelativePath(fileLocation, file)}";
                            var fileName = Path.GetFileName(file);
                            byte[] bs = File.ReadAllBytes(file);
                            using (var filestream = new MemoryStream(bs))
                            {
                                var fileInfo = new FileInfo(file);
                                var metaData = new Dictionary<string, string>
                                {
                                            { "Test-Metadata", "Test  Test" }
                                };
                                await Client.PutObjectAsync(
                                    bucketName,
                                    relativePath,
                                    file,
                                    "application/octet-stream",
                                    metaData);
                            }
                        }
                    }
                    else
                    {
                        byte[] bs = File.ReadAllBytes(fileLocation);
                        using (MemoryStream filestream = new MemoryStream(bs))
                        {
                            FileInfo fileInfo = new FileInfo(fileLocation);
                            var metaData = new Dictionary<string, string>
                        {
                                    { "Test-Metadata", "Test  Test" }
                        };
                            await Client.PutObjectAsync(
                                bucketName,
                                objectName,
                                fileLocation,
                                "application/octet-stream",
                                metaData);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"[Bucket]  Exception: {e}");
                }
            });
        }

        public async Task GetFile(string bucketName, string objectName, string fileName)
        {
            await Client.GetObjectAsync(bucketName, objectName, fileName);
        }

        public async Task DeleteBucket(string bucketName)
        {
            bool found = await Client.BucketExistsAsync(bucketName);
            if (found)
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    await Client.RemoveBucketAsync(bucketName);
                });
            }
        }

        public async Task RemoveObjects(string bucketName, string objectName)
        {
            bool found = await Client.BucketExistsAsync(bucketName);
            if (found)
            {
                await Client.RemoveObjectAsync(bucketName, objectName);
            }
        }
    }
}
