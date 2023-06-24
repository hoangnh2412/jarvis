using System;

namespace Infrastructure.File.Minio
{
    public class MinioOption
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string SessionToken { get; set; }
        public string BucketName { get; set; }
    }
}
