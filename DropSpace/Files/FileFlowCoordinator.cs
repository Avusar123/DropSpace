using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;

namespace DropSpace.Files
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
            this.chunkSize = configuration.GetValue<int>("ChunkSize");
            this.sessionService = sessionService;
            this.fileVault = fileVault;
            this.pendingUploadStore = pendingUploadStore;
        }

        public async Task<byte[]> GetChunkContent(string hash, long startWith)
        {
            return await fileVault.GetFileChunk(hash, startWith, chunkSize);
        }

        public async Task<PendingUploadModel> InitiateNewUpload(InitiateUploadModel initiateUploadModel)
        {

            if (!await sessionService.CanSave(initiateUploadModel.SessionId, initiateUploadModel.Size) ||
                !await fileVault.CanFit(initiateUploadModel.Size))
            {
                throw new NotEnoughSpaceException(initiateUploadModel.Size);
            }

            var pendingupload = new PendingUploadModel
            (
                Guid.NewGuid(),
                initiateUploadModel.Size,
                chunkSize,
                initiateUploadModel.Name,
                initiateUploadModel.SessionId
            );

            await pendingUploadStore.CreateAsync(pendingupload);

            return pendingupload;
        }

        public async Task<PendingUploadModel> SaveNewChunk(UploadChunkModel uploadChunk)
        {
            var upload = await pendingUploadStore.GetById(uploadChunk.UploadId);

            if (!await sessionService.CanSave(upload.SessionId, (int)uploadChunk.File.Length) ||
                !await fileVault.CanFit((int)uploadChunk.File.Length))
            {
                throw new NotEnoughSpaceException((int)uploadChunk.File.Length);
            }

            if (upload.ChunkSize < uploadChunk.File.Length)
            {
                throw new ArgumentException("Объем чанка превышает заданный");
            }

            var stream = new MemoryStream();

            await uploadChunk.File.CopyToAsync(stream);

            stream.Position = 0;

            await fileVault.SaveData(upload.Id.ToString(), stream);

            upload.SendedSize += uploadChunk.File.Length;

            upload.LastChunkUploaded = DateTime.Now;

            if (upload.SendedSize >= upload.ByteSize)
            {
                await pendingUploadStore.DeleteAsync(upload.Id);

                upload.IsCompleted = true;
            }
            else
            {
                await pendingUploadStore.UpdateAsync(upload);
            }

            return upload;
        }
    }
}
