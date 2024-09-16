﻿using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
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
        private readonly IFileSaver fileSaver;
        private readonly IPendingUploadStore pendingUploadStore;
        private readonly int chunkSize;

        public FileFlowCoordinator(
            IConfiguration configuration, 
            ISessionService sessionService,
            IFileSaver fileSaver,
            IPendingUploadStore pendingUploadStore)
        {
            this.chunkSize = configuration.GetValue<int>("ChunkSize");
            this.sessionService = sessionService;
            this.fileSaver = fileSaver;
            this.pendingUploadStore = pendingUploadStore;
        }

        public async Task<PendingUploadModel> InitiateNewUpload(InitiateUploadModel initiateUploadModel)
        {
            bool isCompleted = false;

            if (!await sessionService.CanSave(initiateUploadModel.SessionId, initiateUploadModel.Size) ||
                !await fileSaver.CanFit(initiateUploadModel.Size))
            {
                throw new NotEnoughSpaceException(initiateUploadModel.Size);
            }

            if (await fileSaver.ContainsFileWithHash(initiateUploadModel.Hash))
            {
                isCompleted = await fileSaver.IsFileCompleted(initiateUploadModel.Hash);

                if (!isCompleted)
                {
                    await fileSaver.DeleteAsync(initiateUploadModel.Hash);
                }
            }

            var pendingupload = new PendingUploadModel
                (
                    Guid.NewGuid(),
                    initiateUploadModel.Size,
                    chunkSize,
                    initiateUploadModel.Hash,
                    initiateUploadModel.Name,
                    initiateUploadModel.SessionId
                )
            { IsCompleted = isCompleted };

            if (!isCompleted)
                await pendingUploadStore.CreateAsync(pendingupload);

            return pendingupload;
        }

        public async Task<PendingUploadModel> SaveNewChunk(UploadChunkModel uploadChunk)
        {
            var upload = await pendingUploadStore.GetById(uploadChunk.UploadId);

            if (!await sessionService.CanSave(upload.SessionId, (int)uploadChunk.File.Length) ||
                !await fileSaver.CanFit((int)uploadChunk.File.Length))
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

            await fileSaver.SaveData(upload.FileHash, stream);

            upload.SendedSize += uploadChunk.File.Length;

            upload.LastChunkUploaded = DateTime.Now;

            if (upload.SendedSize >= upload.ByteSize)
            {
                await pendingUploadStore.DeleteAsync(upload.Id);

                if (!await fileSaver.IsFileCompleted(upload.FileHash))
                {
                    throw new HashDoesNotMatch();
                }

                upload.IsCompleted = true;
                
            } else
            {
                await pendingUploadStore.UpdateAsync(upload);
            }

            return upload;
        }
    }
}