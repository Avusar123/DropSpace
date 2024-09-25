namespace DropSpace.Contracts.Dtos
{
    public record SessionDto(Guid Id, string Name, int MembersCount, TimeSpan RemainingTime);
}
