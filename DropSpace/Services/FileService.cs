using DropSpace.Contracts.Dtos;
using DropSpace.Controllers.Filters;
using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Extensions;
using DropSpace.Files.Interfaces;
using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Services.Interfaces;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace DropSpace.Services
{
    public class FileService(
        IFileStore fileStore,
        ISessionStore sessionStore,
        IEventTransmitter eventTransmitter,
        IFileFlowCoordinator fileCoordinator) : IFileService
    {
        public async Task<FileModelDto> CreateUpload(InitiateUploadModel initiateNewUpload)
        {
            var fileId = await fileStore.CreateAsync(new FileModel()
            {
                FileName = initiateNewUpload.Name,
                ByteSize = initiateNewUpload.Size,
                SessionId = initiateNewUpload.SessionId
            });

            var pendingUpload = await fileCoordinator.InitiateNewUpload(initiateNewUpload, fileId);

            //await eventTransmitter.FireEvent(
            //    new FileListChangedEvent()
            //    {
            //        UserIds = (await sessionStore
            //                        .GetAsync(initiateNewUpload.SessionId))
            //                        .GetMemberIds()
            //    });

            var file = await fileStore.GetById(fileId);

            return file.ToDto();

        }

        public async Task Delete(Guid fileId, Guid sessionId)
        {
            var session = await sessionStore.GetAsync(sessionId);

            if (session.Files.Any(file => file.Id == fileId))
            {
                await fileStore.Delete(fileId);

                //await eventTransmitter.FireEvent(
                //    new FileListChangedEvent()
                //    {
                //        UserIds = session.GetMemberIds()
                //    });
            }
        }
        
        public async Task<List<FileModelDto>> GetAllFiles(Guid sessionId)
        {
            return (await fileStore.GetAll(sessionId))
                .Select(file =>
                        file.ToDto()).ToList();
        }

        public async Task<ChunkData> GetChunkData(DownloadChunkModel downloadChunkModel)
        {
            var file = await GetFile(downloadChunkModel.FileId);

            var content = await fileCoordinator.GetChunkContent(downloadChunkModel.FileId.ToString(), downloadChunkModel.StartWith);

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(file.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return new ChunkData()
            {
                ContentType = contentType,
                Content = content,
            };
        }

        public async Task<FileModel> GetFile(Guid fileId)
        {
            return await fileStore.GetById(fileId);
        }

        public async Task<FileModelDto> UploadNewChunk(UploadChunkModel uploadChunkModel)
        {
            var pendingUpload = await fileCoordinator.SaveNewChunk(uploadChunkModel);
                //await eventTransmitter.FireEvent(
                //    new FileListChangedEvent()
                //    {
                //        UserIds = (await sessionStore
                //                        .GetAsync(pendingUpload.SessionId))
                //                        .GetMemberIds()
                //    });

            return pendingUpload.File.ToDto();
        }
    }
}
