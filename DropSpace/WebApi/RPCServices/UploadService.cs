using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Extensions;
using DropSpace.Domain;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Controllers.Filters.MemberFilter;
using DropSpace.WebApi.Utils.Requirements;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using Uploads;

namespace DropSpace.WebApi.RPCServices
{
    [Authorize]
    public class UploadService(IFileService fileService, IAuthorizationService authorizationService) : Upload.UploadBase
    {
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