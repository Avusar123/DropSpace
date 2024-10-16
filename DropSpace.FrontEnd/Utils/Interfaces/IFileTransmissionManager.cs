using DropSpace.Contracts.Dtos;
using Uploads;

namespace DropSpace.FrontEnd.Utils.Interfaces
{
    public interface IFileTransmissionManager
    {
        Task UploadFile(UploadRequest request, Stream fileStream, Action<FileModelDto> uploadCallback);
    }
}