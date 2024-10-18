using DropSpace.Contracts.Dtos;
using Google.Protobuf;
using Uploads;
using static Uploads.Upload;
using DropSpace.Contracts.Extensions;
using DropSpace.FrontEnd.Utils.Interfaces;
using DropSpace.Contracts.Models;
using Microsoft.JSInterop;
using DropSpace.FrontEnd.Utils.ErrorHandlers;
using Grpc.Core;
namespace DropSpace.FrontEnd.Utils
{
    public record DownloadModel(
        DownloadRequest DownloadRequest, 
        Func<DownloadRequest,ChunkResponse, Task> DownloadProgressCallback,
        Func<DownloadRequest, Task>? DownloadStartedCallback = null);

    public class FileTransmissionManager(
        UploadClient uploadClient,
        ErrorHandlerFactory errorHandlerFactory) : IFileTransmissionManager
    {
        Queue<DownloadModel> pendingDownloads = new();

        bool alreadyDownloading = false;

        public Task QueueFileToDownload(DownloadModel downloadModel)
        {
            if (!Guid.TryParse(downloadModel.DownloadRequest.FileId, out _))
            {
                throw new ArgumentException("FileId не GUID");
            }

            pendingDownloads.Enqueue(downloadModel);

            if (!alreadyDownloading)
            {
                alreadyDownloading = true;

                _ = Task.Run(ProcessDownload);
            }

            return Task.CompletedTask;
        }

        public async Task UploadFile(UploadRequest request, Stream fileStream, Action<FileModelDto> uploadCallback)
        {
            var fileModel = (await uploadClient.UploadFileAsync(new FileData()
            {
                UploadRequest = request
            })).ToDto();

            if (fileModel.Upload == null)
            {
                throw new NullReferenceException("Загрузка не найдена!");
            }

            var chunkSize = fileModel.Upload.ChunkSize;

            byte[] buffer = new byte[Math.Min(chunkSize, fileStream.Length)];

            while (await fileStream.ReadAsync(buffer) > 0)
            {
                fileModel = (await uploadClient.UploadFileAsync(new FileData()
                {
                    Chunk = new PushChunkRequest()
                    {
                        Data = ByteString.CopyFrom(buffer),
                        FileId = fileModel.Id.ToString(),
                    }
                })).ToDto();

                uploadCallback.Invoke(fileModel);

                if (fileModel.IsUploaded)
                {
                    return;
                }

                buffer = new byte[Math.Min(chunkSize, fileStream.Length - fileModel.Upload!.SendedSize)];
            }
        }

        private async Task ProcessDownload()
        {
            try
            {
                while (pendingDownloads.TryDequeue(out var downloadModel))
                {
                    var model = new DownloadRequest()
                    {
                        FileId = downloadModel.DownloadRequest.FileId
                    };

                    using var downloadCall = uploadClient.DownloadFile(model);


                    if (downloadModel.DownloadStartedCallback != null)
                    {
                        await downloadModel.DownloadStartedCallback.Invoke(model);
                    }
                    

                    while (await downloadCall.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        var chunkResponse = downloadCall.ResponseStream.Current;

                        await downloadModel
                            .DownloadProgressCallback
                            .Invoke(
                                downloadModel.DownloadRequest, 
                                chunkResponse);
                    }
                }
            } catch (RpcException ex)
            {
                await errorHandlerFactory.MessageError.HandleAsync(ex.Message);
            }
            finally
            {
                alreadyDownloading = false;
            }
        }
    }
}
