using DropSpace.Domain;
using Uploads;

namespace DropSpace.Infrastructure.Stores.Interfaces
{
    public interface IUploadStateStore
    {
        Task SetAsync(UploadState model, Guid fileId);
        Task DeleteAsync(Guid fileId);
        Task<UploadState?> GetByFileId(Guid fileId);
    }
}
