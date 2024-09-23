﻿using DropSpace.Contracts.Dtos;
using DropSpace.Models;
using DropSpace.Models.Data;

namespace DropSpace.Services.Interfaces
{
    public interface IFileService
    {
        Task<List<FileModelDto>> GetAllFiles(Guid sessionId);

        Task<List<PendingUploadModelDto>> GetAllUploads(Guid sessionId);

        Task<FileModel> GetFile(Guid fileId);

        Task<PendingUploadModelDto> CreateUpload(InitiateUploadModel initiateNewUpload);

        Task<PendingUploadModelDto> UploadNewChunk(UploadChunkModel uploadChunkModel);

        Task<ChunkData> GetChunkData(DownloadChunkModel downloadChunkModel);

        Task Delete(Guid fileId, Guid sessionId);
    }
}
