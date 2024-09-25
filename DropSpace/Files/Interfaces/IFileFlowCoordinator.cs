using DropSpace.Models;
using DropSpace.Models.Data;

namespace DropSpace.Files.Interfaces
{
    public interface IFileFlowCoordinator
    {
        Task<PendingUploadModel> InitiateNewUpload(InitiateUploadModel initiateUploadModel);

        Task<PendingUploadModel> SaveNewChunk(UploadChunkModel uploadChunsk);

        Task<byte[]> GetChunkContent(string fileId, long startWith);
    }
}
