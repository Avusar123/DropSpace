namespace DropSpace.Models.Data
{
    public class FileModel
    {
        public Guid Id { get; set; }

        public long ByteSize { get; set; }

        public string FileName { get; set; }

        public PendingUploadModel PendingUpload { get; set; }

        public Guid SessionId { get; set; }

        public Session Session { get; set; }
    }
}