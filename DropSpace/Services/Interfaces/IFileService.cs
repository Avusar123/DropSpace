using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;

namespace DropSpace.Services.Interfaces
{
    public interface IFileService
    {
        Task<List<FileModelDto>> GetAllFiles(Guid sessionId);

        Task<List<PendingUploadModelDto>> GetAllUploads(Guid sessionId);

        Task<FileModel> GetFile(Guid fileId);

        Task<PendingUploadModelDto> CreateUpload(InitiateUploadModel initiateNewUpload);

        Task<PendingUploadModelDto> UploadNewChunk(UploadChunkModel uploadChunkModel);

        Task Delete(Guid fileId, Guid sessionId);
    }
}
