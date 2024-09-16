using DropSpace.Models.Data;

namespace DropSpace.Stores.Interfaces
{
    public interface IFileStore
    {

        Task<Guid> CreateAsync(FileModel fileModel);

        Task Delete(Guid id);

        Task Update(Session session);

        Task<FileModel> GetById(Guid id);

        Task<List<FileModel>> GetAll(Guid sessionId);
    }
}
