namespace DropSpace.Models.Data
{
    public class Session
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public List<FileModel> Files { get; set; }

        public int MaxSize { get; set; }

        public DateTime Created { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime GetExpiresAt()
        {
            return Created + Duration;
        }

    }
}
