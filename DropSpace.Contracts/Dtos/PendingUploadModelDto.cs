namespace DropSpace.Contracts.Dtos
{
    public record PendingUploadModelDto(
        Guid Id,
        long ChunkSize,
        long SendedSize,
        double SendedSizeMb,
        long Size,
        double SizeMb,
        string FileName,
        bool IsCompleted);
}
