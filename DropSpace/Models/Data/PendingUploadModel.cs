namespace DropSpace.Models.Data
{
    public class PendingUploadModel
    {
        public Guid Id { get; set; }

        public long ByteSize { get; set; }

        public DateTime LastChunkUploaded { get; set; }

        public bool IsCompleted { get; set; }

        public int RemainingChunks => (int)Math.Ceiling((decimal)(ByteSize - SendedSize) / ChunkSize);

        public long SendedSize { get; set; }

        public long ChunkSize { get; set; }

        public string FileName { get; set; }

        public Guid SessionId { get; set; }

        public Session Session { get; set; }

        public PendingUploadModel(
            Guid id,
            long byteSize,
            long chunkSize,
            string fileName,
            Guid sessionId)
        {
            this.Id = id;
            this.ByteSize = byteSize;
            this.ChunkSize = chunkSize;
            this.FileName = fileName;
            this.SessionId = sessionId;
            IsCompleted = false;
            SendedSize = 0;
        }
    }
}
