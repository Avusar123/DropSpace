using DropSpace.Contracts.Dtos;
using DropSpace.Models.Data;

namespace DropSpace.Extensions
{
    public static class PendingUploadExtensions
    {
        public static PendingUploadModelDto ToDto(this PendingUploadModel pendingUpload)
        {
            return new PendingUploadModelDto(
                pendingUpload.Id,
                pendingUpload.ChunkSize,
                pendingUpload.SendedSize,
                pendingUpload.SendedSize.ToMBytes(),
                pendingUpload.ByteSize,
                pendingUpload.ByteSize.ToMBytes(),
                pendingUpload.FileName,
                pendingUpload.IsCompleted);
        }
    }
}
