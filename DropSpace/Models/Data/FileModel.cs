namespace DropSpace.Models.Data
{
    public class FileModel
    {
        public Guid Id { get; set; }

        public int Size { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public Guid SessionId { get; set; }
    }
}