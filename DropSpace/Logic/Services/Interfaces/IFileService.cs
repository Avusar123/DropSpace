using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using Uploads;

namespace DropSpace.Logic.Services.Interfaces
{
    public interface IFileService
    {
        Task<List<FileModelDto>> GetAllFiles(Guid sessionId);

        Task<FileModelDto> GetFile(Guid fileId);

        Task<Guid> GetSessionId(Guid fileId);

        Task<FileModelDto> CreateFile(UploadRequest uploadRequest);

        Task<FileModelDto> UploadChunk(Guid fileId, byte[] bytes);

        Task<ChunkData> GetChunkData(DownloadChunkModel downloadChunkModel);

        Task Delete(Guid fileId);
    }
}
