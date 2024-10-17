using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using Refit;

namespace DropSpace.FrontEnd.Services
{
    public interface IFileService
    {
        [Post("/File/")]
        Task<FileModelDto> CreateUpload(InitiateUploadModel initiateUploadModel);

        [Delete("/File/")]
        Task Delete([Body] DeleteFileModel deleteFileModel);
        //Task<ActionResult> DownloadFileChunk(DownloadChunkModel downloadChunkModel);

        [Get("/File/All/{sessionId}")]
        Task<List<FileModelDto>> GetAll(Guid sessionId);

        [Get("/File/{fileId}/")]
        Task<FileModelDto> GetFileInfo(Guid fileId);

        [Put("/File/")]
        Task<FileModelDto> UploadChunk(UploadChunkModel uploadChunk);
    }
}