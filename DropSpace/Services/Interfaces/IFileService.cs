using DropSpace.Contracts.Dtos;
using DropSpace.Models;
using DropSpace.Models.Data;

namespace DropSpace.Services.Interfaces
{
    public interface IFileService
    {
        Task<List<FileModelDto>> GetAllFiles(Guid sessionId);

        Task<FileModel> GetFile(Guid fileId);

        Task<FileModelDto> CreateUpload(InitiateUploadModel initiateNewUpload);

        Task<FileModelDto> UploadNewChunk(UploadChunkModel uploadChunkModel);

        Task<ChunkData> GetChunkData(DownloadChunkModel downloadChunkModel);

        Task Delete(Guid fileId, Guid sessionId);
    }
}
