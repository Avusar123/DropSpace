using DropSpace.Contracts.Models;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Services.Interfaces;

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
        private readonly IPendingUploadStore pendingUploadStore;
        private readonly int chunkSize;

        public FileFlowCoordinator(
            IConfiguration configuration,
            ISessionService sessionService,
            IFileVault fileVault,
            IPendingUploadStore pendingUploadStore)
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

        public async Task<PendingUploadModel> InitiateNewUpload(InitiateUploadModel initiateUploadModel, Guid fileId)
        {

            if (!await sessionService.CanSave(initiateUploadModel.SessionId, initiateUploadModel.Size) ||
                !await fileVault.CanFit(initiateUploadModel.Size))
            {
                throw new NotEnoughSpaceException(initiateUploadModel.Size);
            }

            var pendingupload = new PendingUploadModel()
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                IsCompleted = false,
                ChunkSize = chunkSize,
                SendedSize = 0,
            };

            await pendingUploadStore.CreateAsync(pendingupload);

            return pendingupload;
        }

        public async Task<PendingUploadModel> SaveNewChunk(UploadChunkModel uploadChunk)
        {
            var upload = await pendingUploadStore.GetById(uploadChunk.UploadId);

            if (!await sessionService.CanSave(upload.File.SessionId, uploadChunk.Chunk.LongLength) ||
                !await fileVault.CanFit(uploadChunk.Chunk.LongLength))
            {
                throw new NotEnoughSpaceException(uploadChunk.Chunk.LongLength);
            }

            if (upload.ChunkSize < uploadChunk.Chunk.LongLength ||
                upload.SendedSize + uploadChunk.Chunk.LongLength > upload.File.ByteSize)
            {
                throw new ArgumentException("Объем загрузки превышен!");
            }

            var stream = new MemoryStream();

            await stream.WriteAsync(uploadChunk.Chunk);

            stream.Position = 0;

            await fileVault.SaveData(upload.FileId.ToString(), stream);

            upload.SendedSize += uploadChunk.Chunk.LongLength;

            upload.LastChunkUploaded = DateTime.UtcNow;

            if (upload.SendedSize >= upload.File.ByteSize)
            {
                upload.IsCompleted = true;
            }

            await pendingUploadStore.UpdateAsync(upload);

            return upload;
        }
    }
}
