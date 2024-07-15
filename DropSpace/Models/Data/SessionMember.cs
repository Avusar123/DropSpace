namespace DropSpace.Models.Data
{
    public class SessionMember
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public Guid SessionId { get; set; }
    }
}
