using System;

namespace Jarvis.Core.Models.FileStorage
{
    public class FileUploadOutput
    {
        public Guid Key { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string BucketName { get; set; }
        public string Metadata { get; set; }
    }
}