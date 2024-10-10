using DropSpace.Contracts.Models;
using DropSpace.Domain;

namespace DropSpace.Logic.Files.Interfaces
{
    public interface IFileFlowCoordinator
    {
        Task<PendingUploadModel> InitiateNewUpload(InitiateUploadModel initiateUploadModel, Guid fileId);

        Task<PendingUploadModel> SaveNewChunk(UploadChunkModel uploadChunsk);

        Task<byte[]> GetChunkContent(string fileId, long startWith);
    }
}
