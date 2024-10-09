namespace DropSpace.Domain
{
    public class Session
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<FileModel> Files { get; set; } = [];

        public long MaxSize { get; set; }

        public DateTime Created { get; set; }

        public TimeSpan Duration { get; set; }

        public List<SessionMember> Members { get; set; } = [];
    }
}
