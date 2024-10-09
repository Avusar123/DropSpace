using DropSpace.Domain;
using DropSpace.Domain.Models;
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

            if (!await sessionService.CanSave(upload.File.SessionId, (int)uploadChunk.File.Length) ||
                !await fileVault.CanFit((int)uploadChunk.File.Length))
            {
                throw new NotEnoughSpaceException((int)uploadChunk.File.Length);
            }

            if (upload.ChunkSize < uploadChunk.File.Length ||
                upload.SendedSize + uploadChunk.File.Length > upload.File.ByteSize)
            {
                throw new ArgumentException("Объем загрузки превышен!");
            }

            var stream = new MemoryStream();

            await uploadChunk.File.CopyToAsync(stream);

            stream.Position = 0;

            await fileVault.SaveData(upload.FileId.ToString(), stream);

            upload.SendedSize += uploadChunk.File.Length;

            upload.LastChunkUploaded = DateTime.Now;

            if (upload.SendedSize >= upload.File.ByteSize)
            {
                upload.IsCompleted = true;
            }

            await pendingUploadStore.UpdateAsync(upload);

            return upload;
        }
    }
}
