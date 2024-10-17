using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.Logic.Utils.Converters.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Uploads;

namespace DropSpace.Logic.Services
{
    public class FileService(
        IFileStore fileStore,
        ISessionStore sessionStore,
        IEventTransmitter eventTransmitter,
        IFileFlowCoordinator fileCoordinator,
        IFileConverter fileConverter,
        IFileVault fileVault,
        ILogger<FileService> logger) : IFileService
    {
        public async Task<FileModelDto> CreateFile(UploadRequest uploadRequest)
        {
            logger.LogInformation("Начата загрузка файла с именем {name}", uploadRequest.FileName);

            using var transaction = await fileStore
                .ApplicationContext
                .Database
                .BeginTransactionAsync();

            var fileModel = await fileStore.CreateAsync(new Domain.FileModel()
            {
                FileName = uploadRequest.FileName,
                ByteSize = uploadRequest.Size,
                SessionId = Guid.Parse(uploadRequest.SessionId),
                IsUploaded = false
            });

            var uploadState = await fileCoordinator.InitiateUpload(uploadRequest, fileModel.Id);

            var session = await sessionStore.GetAsync(Guid.Parse(uploadRequest.SessionId));

            var fullfileDto = fileConverter.ConvertToDto(fileModel, uploadState);

            await eventTransmitter.FireEvent(
                new FileUpdatedEvent(session.GetMemberIds(), fileConverter.ConvertToDto(fileModel))
            );

            await transaction.CommitAsync();

            logger.LogInformation("Загрузка файла с именем {name} успешно сохранена в БД", uploadRequest.FileName);

            return fullfileDto;
        }

        public async Task Delete(Guid fileId)
        {
            var file = await fileStore.GetById(fileId);

            await eventTransmitter.FireEvent(
                new FileDeletedEvent(file.Session.GetMemberIds(), fileId)
            );

            await fileStore.Delete(fileId);

            await fileVault.DeleteAsync(fileId.ToString());
        }

        public async Task<List<FileModelDto>> GetAllFiles(Guid sessionId)
        {
            return (await fileStore.GetAll(sessionId))
                .Select(file => fileConverter.ConvertToDto(file)).ToList();
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

        public Task<FileModelDto> GetFile(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public async Task<FileModelDto> UploadChunk(Guid fileId, byte[] data)
        {
            logger.LogInformation("Начато сохранение нового чанка для загрузки файла {file}", fileId);

            var file = await fileStore.GetById(fileId);

            var uploadState = await fileCoordinator.SaveChunk(
                new UploadChunkRequest(
                    fileId,
                    file.ByteSize, 
                    data)
                );

            logger.LogInformation("Чанк успешно сохранен для загрузки файла {file}", fileId);

            file.IsUploaded = uploadState.IsCompleted;

            await fileStore.ApplicationContext.SaveChangesAsync();

            var fileDto = fileConverter.ConvertToDto(file, uploadState.IsCompleted ? null : uploadState);

            if (file.IsUploaded)
            {
                await eventTransmitter.FireEvent(
                    new FileUpdatedEvent(file.Session.GetMemberIds(), fileDto)
                );
            }

            return fileDto;
        }
    }
}
