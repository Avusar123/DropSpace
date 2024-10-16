using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Extensions;
using DropSpace.Domain;
using DropSpace.Logic.Services.Interfaces;
using Grpc.Core;
using System.Runtime.InteropServices;
using Uploads;

namespace DropSpace.WebApi.RPCServices
{
    public class UploadService(IFileService fileService) : Upload.UploadBase
    {
        public async override Task<Uploads.FileModel> UploadFile(FileData request, ServerCallContext context)
        {
            return request.PayloadCase switch
            {
                FileData.PayloadOneofCase.UploadRequest => (await fileService.CreateFile(request.UploadRequest)).ToRPCModel(),
                FileData.PayloadOneofCase.Chunk => (await fileService
                                                    .UploadChunk(
                                                        Guid.Parse(request.Chunk.FileId),
                                                        request.Chunk.Data.ToByteArray()))
                                                    .ToRPCModel(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}