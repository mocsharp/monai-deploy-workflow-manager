namespace Monai.Deploy.WorkloadManager.IntegrationTests.POCO
{
    static class TestExecutionConfig
    {
        public static class RabbitConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }

            public static string WorkflowRequestQueue { get; set; }

            public static string TaskDispatchQueue { get; set; }

            public static string TaskCallbackQueue { get; set; }

            public static string WorkflowCompleteQueue { get; set; }
        }

        public static class MongoConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string User { get; set; }

            public static string Password { get; set; }

            public static string Database { get; set; }

            public static string Collection { get; set; }
        }

        public static class MinioConfig
        {
            public static string Host { get; set; }

            public static int Port { get; set; }

            public static string AccessKey { get; set; }

            public static string SecretKey { get; set; }
        }
    }
}
