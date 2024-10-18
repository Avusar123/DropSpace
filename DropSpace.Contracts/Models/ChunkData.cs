namespace DropSpace.Contracts.Models
{
    public class ChunkData
    {
        public byte[] Content { get; set; }

        public string ContentType { get; set; }

        public bool FileEnded { get; set; }
    }
}
