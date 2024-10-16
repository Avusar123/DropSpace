using Uploads;

namespace DropSpace.Logic.Files.Interfaces
{
    public record UploadChunkRequest(Guid FileId, long FileSize, byte[] Chunk);

    public interface IFileFlowCoordinator
    {
        Task<UploadState> InitiateUpload(UploadRequest uploadRequest, Guid fileId);

        Task<UploadState> SaveChunk(UploadChunkRequest uploadChunkRequest);

        Task<byte[]> GetChunkContent(string fileId, long startWith);
    }
}
