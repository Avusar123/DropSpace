namespace DropSpace.Contracts.Dtos
{
    public record class FileModelDto(Guid Id, long Size, double SizeMb, string FileName, PendingUploadModelDto Upload);
}
