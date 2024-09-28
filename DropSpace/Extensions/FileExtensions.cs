using DropSpace.Contracts.Dtos;
using DropSpace.Models.Data;

namespace DropSpace.Extensions
{
    public static class FileExtensions
    {
        public static FileModelDto ToDto(this FileModel file)
        {
            return new FileModelDto(
                file.Id,
                file.ByteSize,
                file.ByteSize.ToMBytes(),
                file.FileName,
                file.PendingUpload.ToDto());
        }
    }
}
