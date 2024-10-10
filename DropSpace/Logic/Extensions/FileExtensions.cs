using DropSpace.Contracts.Dtos;
using DropSpace.Domain;

namespace DropSpace.Logic.Extensions
{
    public static class FileExtensions
    {
        public static FileModelDto ToDto(this FileModel file)
        {
            var pendingUpload = file.PendingUpload;

            return new FileModelDto(
                file.Id,
                file.ByteSize,
                file.ByteSize.ToMBytes(),
                file.FileName,
                file.PendingUpload.ToDto());
        }
    }
}
