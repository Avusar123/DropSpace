namespace DropSpace.Models.Data
{
    public class PendingUploadModel
    {
        public Guid Id { get; set; }

        public DateTime LastChunkUploaded { get; set; }

        public bool IsCompleted { get; set; }

        public long SendedSize { get; set; }

        public long ChunkSize { get; set; }

        public FileModel File { get; set; }

        public Guid FileId { get; set; }
    }
}
