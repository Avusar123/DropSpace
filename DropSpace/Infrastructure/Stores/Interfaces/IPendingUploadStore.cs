using DropSpace.Domain;

namespace DropSpace.Infrastructure.Stores.Interfaces
{
    public interface IPendingUploadStore
    {
        Task<Guid> CreateAsync(PendingUploadModel model);
        Task UpdateAsync(PendingUploadModel model);
        Task DeleteAsync(Guid id);
        Task<PendingUploadModel> GetById(Guid id);
    }
}
