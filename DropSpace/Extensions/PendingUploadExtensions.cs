using DropSpace.Contracts.Dtos;
using DropSpace.Models.Data;

namespace DropSpace.Extensions
{
    public static class PendingUploadExtensions
    {
        public static PendingUploadModelDto ToDto(this PendingUploadModel upload)
        {
            return new PendingUploadModelDto(upload.Id, upload.ChunkSize, upload.SendedSize, upload.SendedSize.ToMBytes(), upload.IsCompleted);
        }
    }
}
