using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace DropSpace.Logic.Services
{
    public class FileService(
        IFileStore fileStore,
        ISessionStore sessionStore,
        IEventTransmitter eventTransmitter,
        IFileFlowCoordinator fileCoordinator,
        ILogger<FileService> logger) : IFileService
    {
        public async Task<FileModelDto> CreateUpload(InitiateUploadModel initiateNewUpload)
        {
            logger.LogInformation("Начата загрузка файла с именем {name}", initiateNewUpload.Name);

            Guid fileId;

            using(var transaction = await fileStore.ApplicationContext.Database.BeginTransactionAsync())
            {
                fileId = await fileStore.CreateAsync(new FileModel()
                {
                    FileName = initiateNewUpload.Name,
                    ByteSize = initiateNewUpload.Size,
                    SessionId = initiateNewUpload.SessionId
                });

                await fileCoordinator.InitiateNewUpload(initiateNewUpload, fileId);

                await transaction.CommitAsync();
            }

            var file = await fileStore.GetById(fileId);

            await eventTransmitter.FireEvent(
                new FileUpdatedEvent(file.Session.GetMemberIds(), file.ToDto())
            );

            logger.LogInformation("Загрузка файла с именем {name} успешно сохранена в БД", initiateNewUpload.Name);

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
            logger.LogInformation("Начато сохранение нового чанка для загрузки {upload}", uploadChunkModel.UploadId);

            var pendingUpload = await fileCoordinator.SaveNewChunk(uploadChunkModel);

            logger.LogInformation("Чанк успешно сохранен для загрузки {upload}", uploadChunkModel.UploadId);

            await eventTransmitter.FireEvent(
                new FileUpdatedEvent(pendingUpload.File.Session.GetMemberIds(), pendingUpload.File.ToDto())
            );

            return pendingUpload.File.ToDto();
        }
    }
}
