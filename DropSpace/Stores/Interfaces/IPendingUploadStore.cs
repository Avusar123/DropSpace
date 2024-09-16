using DropSpace.Models.Data;

namespace DropSpace.Stores.Interfaces
{
    public interface IPendingUploadStore
    {
        Task<Guid> CreateAsync(PendingUploadModel model);
        Task UpdateAsync(PendingUploadModel model);
        Task DeleteAsync(Guid id);
        Task<List<PendingUploadModel>> GetAll(Guid sessionId);
        Task<PendingUploadModel> GetById(Guid id);
    }
}
