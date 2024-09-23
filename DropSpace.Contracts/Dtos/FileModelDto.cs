namespace DropSpace.Contracts.Dtos
{
    public record class FileModelDto(Guid Id, double Size, double SizeMb, string FileName);
}
