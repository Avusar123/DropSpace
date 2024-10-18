using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Extensions;
using DropSpace.Contracts.Models;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Controllers.Filters.MemberFilter;
using DropSpace.WebApi.Utils.Requirements;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using Uploads;

namespace DropSpace.WebApi.RPCServices
{
    [Authorize]
    public class UploadService(
        IFileService fileService,
        IAuthorizationService authorizationService) : Upload.UploadBase
    {

        public async override Task DownloadFile(
            DownloadRequest request, 
            IServerStreamWriter<ChunkResponse> 
            responseStream, ServerCallContext context)
        {
            try
            {
                var fileId = Guid.Parse(request.FileId);

                var sessionId = await fileService.GetSessionId(fileId);

                if (!(await authorizationService.AuthorizeAsync(
                                context.GetHttpContext().User,
                                sessionId,
                                new MemberRequirement())).Succeeded)
                {
                    throw new ArgumentException("У пользователя нет доступа к сессии!");
                }

                ChunkData chunkData;

                long offset = 0;

                do
                {
                    var model = new DownloadChunkModel()
                    {
                        FileId = fileId,
                        StartWith = offset
                    };

                    chunkData = await fileService.GetChunkData(model);

                    await responseStream.WriteAsync(
                        new ChunkResponse()
                        {
                            Data = ByteString.CopyFrom(chunkData.Content),
                            FileEnded = chunkData.FileEnded,
                            FileType = chunkData.ContentType
                        });

                    offset += chunkData.Content.LongLength;
                } while (!chunkData.FileEnded);
            } catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.GetType().Name), ex.Message);
            }
            
        }
        public async override Task<Uploads.FileModel> UploadFile(FileData request, ServerCallContext context)
        {
            switch (request.PayloadCase)
            {
                case FileData.PayloadOneofCase.UploadRequest:
                    if ((await authorizationService.AuthorizeAsync(
                            context.GetHttpContext().User,
                            Guid.Parse(request.UploadRequest.SessionId),
                            new MemberRequirement())).Succeeded)
                    {
                        var response = (await fileService.CreateFile(request.UploadRequest)).ToRPCModel();

                        return response;
                    }
                    else
                    {
                        throw new ArgumentException("У пользователя нет доступа к сессии!");
                    }

                case FileData.PayloadOneofCase.Chunk:
                    return (await fileService
                                    .UploadChunk(
                                        Guid.Parse(request.Chunk.FileId),
                                        request.Chunk.Data.ToByteArray())).ToRPCModel();
                default: throw new NotImplementedException();
            }
        }
    }
}