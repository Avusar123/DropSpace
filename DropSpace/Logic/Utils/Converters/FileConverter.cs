using DropSpace.Contracts.Dtos;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Utils.Converters.Interfaces;
using Uploads;

namespace DropSpace.Logic.Utils.Converters
{
    public class FileConverter : IFileConverter
    {
        public FileModelDto ConvertToDto(Domain.FileModel fileModel, UploadState? state = null)
        {
            if (state != null)
            {
                state.SendedSizeMb = state.SendedSize.ToMBytes();
            }

            return new FileModelDto(
                fileModel.Id, 
                fileModel.ByteSize, 
                fileModel.ByteSize.ToMBytes(),
                fileModel.FileName,
                fileModel.IsUploaded,
                state);
        }
    }
}
