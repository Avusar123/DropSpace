namespace DropSpace.Contracts.Dtos
{
    public record PendingUploadModelDto(
        Guid Id,
        long ChunkSize,
        long SendedSize,
        double SendedSizeMb,
        bool IsCompleted);
}
