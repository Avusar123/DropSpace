using DropSpace.Contracts.Dtos;

namespace DropSpace.Contracts.Extensions
{
    public static class FileModelDtoExtensions
    {
        public static Uploads.FileModel ToRPCModel(this FileModelDto fileModelDto)
        {
            return new Uploads.FileModel()
            {
                FileName = fileModelDto.FileName,
                Size = fileModelDto.Size,
                Id = fileModelDto.Id.ToString(),
                SizeMb = fileModelDto.SizeMb,
                UploadState = fileModelDto.Upload
            };
        }

        public static FileModelDto ToDto(this Uploads.FileModel fileModel)
        {
            return new FileModelDto(
                Guid.Parse(fileModel.Id), 
                fileModel.Size,
                fileModel.SizeMb, 
                fileModel.FileName, 
                fileModel.UploadState == null, 
                fileModel.UploadState);
        }
    }
}
