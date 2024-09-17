namespace DropSpace.Models.DTOs
{
    public record class FileModelDto(Guid Id, double Size, double SizeMb, string FileName);
}
