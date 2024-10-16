using DropSpace.Contracts.Dtos;
using Google.Protobuf;
using Uploads;
using static Uploads.Upload;
using DropSpace.Contracts.Extensions;
using DropSpace.FrontEnd.Utils.Interfaces;
namespace DropSpace.FrontEnd.Utils
{
    public class FileTransmissionManager(UploadClient uploadClient) : IFileTransmissionManager
    {
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
    }
}
