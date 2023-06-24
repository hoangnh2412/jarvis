namespace Jarvis.Core.Models.FileStorage
{
    public class FileDownloadOutput
    {
        public Database.Poco.File FileStorage { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string BucketName { get; set; }
        public byte[] Content { get; set; }
    }
}