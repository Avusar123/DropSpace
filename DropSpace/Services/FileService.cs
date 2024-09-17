using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Models.DTOs;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DropSpace.Services
{
    public class FileService(
        IFileStore fileStore,
        ISessionStore sessionStore,
        IEventTransmitter eventTransmitter,
        IPendingUploadStore pendingUploadStore,
        IFileFlowCoordinator fileCoordinator) : IFileService
    {
        public async Task<PendingUploadModelDto> CreateUpload(InitiateUploadModel initiateNewUpload)
        {
            var pendingUpload = await fileCoordinator.InitiateNewUpload(initiateNewUpload);

            if (pendingUpload.IsCompleted)
            {
                var files = await fileStore.GetAll(initiateNewUpload.SessionId);

                if (files.FirstOrDefault(file => file.FileHash == initiateNewUpload.Hash) == null)
                {
                    var fileModel = new FileModel()
                    {
                        FileHash = pendingUpload.FileHash,
                        FileName = pendingUpload.FileName,
                        ByteSize = pendingUpload.ByteSize,
                        SessionId = pendingUpload.SessionId
                    };

                    await fileStore.CreateAsync(fileModel);
                }
            }



            await eventTransmitter.FireEvent(
                new FileListChangedEvent()
                {
                    UserIds = (await sessionStore
                                    .GetAsync(initiateNewUpload.SessionId))
                                    .GetMemberIds()
                });

            return pendingUpload.ToDto();
            
        }

        public async Task Delete(Guid fileId, Guid sessionId)
        {
            var session = await sessionStore.GetAsync(sessionId);

            if (session.Files.Any(file => file.Id == fileId))
            {
                await fileStore.Delete(fileId);

                await eventTransmitter.FireEvent(
                    new FileListChangedEvent()
                    {
                        UserIds = session.GetMemberIds()
                    });
            }
        }

        public async Task<List<FileModelDto>> GetAllFiles(Guid sessionId)
        {
            return (await fileStore.GetAll(sessionId))
                .Select(file =>
                        new FileModelDto(file.Id, file.ByteSize.ToMBytes(), file.FileName)).ToList();
        }

        public async Task<List<PendingUploadModelDto>> GetAllUploads(Guid sessionId)
        {
            return (await pendingUploadStore.GetAll(sessionId))
                .Select(up => up.ToDto()).ToList();
        }

        public async Task<FileModel> GetFile(Guid fileId)
        {
            return await fileStore.GetById(fileId);
        }

        public async Task<PendingUploadModelDto> UploadNewChunk(UploadChunkModel uploadChunkModel)
        {
            var pendingUpload = await fileCoordinator.SaveNewChunk(uploadChunkModel);

            if (pendingUpload.IsCompleted)
            {
                var fileModel = new FileModel()
                {
                    FileHash = pendingUpload.FileHash,
                    FileName = pendingUpload.FileName,
                    ByteSize = pendingUpload.ByteSize,
                    SessionId = pendingUpload.SessionId
                };

                await fileStore.CreateAsync(fileModel);

                await eventTransmitter.FireEvent(
                    new FileListChangedEvent()
                    {
                        UserIds = (await sessionStore
                                        .GetAsync(pendingUpload.SessionId))
                                        .GetMemberIds()
                    });
            }
            else
            {
                await eventTransmitter.FireEvent(
                    new NewChunkUploadedEvent()
                    {
                        UserIds = (await sessionStore
                                        .GetAsync(pendingUpload.SessionId))
                                        .GetMemberIds(),


                        Upload = pendingUpload.ToDto()
                    });
            }

            return pendingUpload.ToDto();
        }
    }
}
