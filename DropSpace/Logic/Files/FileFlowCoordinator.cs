using DropSpace.Contracts.Models;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using Uploads;

namespace DropSpace.Logic.Files
{
    public class NotEnoughSpaceException(long size) : Exception($"В сессии недостаточно места для сохранения {size.ToMBytes()} Мбайт")
    {
    }

    public class HashDoesNotMatch() : Exception($"Хеш не совпадает")
    {
    }

    public class FileFlowCoordinator : IFileFlowCoordinator
    {
        private readonly ISessionService sessionService;
        private readonly IFileVault fileVault;
        private readonly IUploadStateStore pendingUploadStore;
        private readonly int chunkSize;

        public FileFlowCoordinator(
            IConfiguration configuration,
            ISessionService sessionService,
            IFileVault fileVault,
            IUploadStateStore pendingUploadStore)
        {
            chunkSize = configuration.GetValue<int>("ChunkSize");
            this.sessionService = sessionService;
            this.fileVault = fileVault;
            this.pendingUploadStore = pendingUploadStore;
        }

        public async Task<byte[]> GetChunkContent(string hash, long startWith)
        {
            return await fileVault.GetFileChunk(hash, startWith, chunkSize);
        }

        public async Task<UploadState> InitiateUpload(UploadRequest uploadRequest, Guid fileId)
        {

            if (!await sessionService.CanSave(Guid.Parse(uploadRequest.SessionId), uploadRequest.Size, fileId) ||
                !await fileVault.CanFit(uploadRequest.Size))
            {
                throw new NotEnoughSpaceException(uploadRequest.Size);
            }

            var pendingupload = new UploadState()
            {
                SendedSize = 0,
                SendedSizeMb = 0,
                ChunkSize = Math.Min(chunkSize, uploadRequest.Size),
                IsCompleted = false
            };

            await pendingUploadStore.SetAsync(pendingupload, fileId);

            return pendingupload;
        }

        public async Task<UploadState> SaveChunk(UploadChunkRequest chunkRequest)
        {
            var upload = await pendingUploadStore.GetByFileId(chunkRequest.FileId);

            if (upload == null)
            {
                throw new NullReferenceException("Загрузка не найдена!");
            }

            if (!await fileVault.CanFit(chunkRequest.Chunk.LongLength))
            {
                throw new NotEnoughSpaceException(chunkRequest.Chunk.LongLength);
            }

            if (upload.ChunkSize < chunkRequest.Chunk.LongLength ||
                upload.SendedSize + chunkRequest.Chunk.LongLength > chunkRequest.FileSize)
            {
                throw new ArgumentException("Объем загрузки превышен!");
            }

            var stream = new MemoryStream();

            await stream.WriteAsync(chunkRequest.Chunk);

            stream.Position = 0;

            await fileVault.SaveData(chunkRequest.FileId.ToString(), stream);

            upload.SendedSize += chunkRequest.Chunk.LongLength;

            if (upload.SendedSize >= chunkRequest.FileSize)
            {
                upload.IsCompleted = true;

                await pendingUploadStore.DeleteAsync(chunkRequest.FileId);

            } else
            {
                await pendingUploadStore.SetAsync(upload, chunkRequest.FileId);
            }

            return upload;
        }
    }
}
