namespace DropSpace.Models.Data
{
    public class Session
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        private List<FileModel> _files = new();

        public IEnumerable<FileModel> Files => _files; 

        public int MaxSize { get; set; }

        public DateTime Created { get; set; }

        public TimeSpan Duration { get; set; }

        public List<SessionMember> Members { get; set; } = [];

        public DateTime GetExpiresAt()
        {
            return Created + Duration;
        }

        public void AttachFileToSession(FileModel file)
        {
            if (!CanSave(file.Size)) 
            {
                throw new ArgumentException("File size cannot be negative");
            }

            _files.Add(file);
        }

        public bool CanSave(int size)
        {
            return Files.Sum(file => file.Size) + size <= MaxSize && size > 0;
        }

        public int GetRemainingSize()
        {
            return MaxSize - Files.Sum(file => file.Size);
        }

    }
}
