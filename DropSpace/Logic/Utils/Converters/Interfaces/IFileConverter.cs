using DropSpace.Contracts.Dtos;
using DropSpace.Domain;
using Uploads;

namespace DropSpace.Logic.Utils.Converters.Interfaces
{
    public interface IFileConverter
    {
        FileModelDto ConvertToDto(Domain.FileModel fileModel, UploadState? state = null);
    }
}
