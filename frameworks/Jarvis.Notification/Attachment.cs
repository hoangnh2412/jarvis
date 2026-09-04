using System.Net.Mime;

namespace Jarvis.Notification;

public class Attachment
{
    public required string FileName { get; set; }
    public byte[] DataContent { get; set; } = [];
    public Stream? StreamContent { get; set; }
    public required ContentType ContentType { get; set; }
}