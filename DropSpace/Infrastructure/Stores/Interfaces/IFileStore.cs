using DropSpace.Domain;

namespace DropSpace.Infrastructure.Stores.Interfaces
{
    public interface IFileStore
    {
        ApplicationContext ApplicationContext { get; }

        Task<Guid> CreateAsync(FileModel fileModel);

        Task Delete(Guid id);

        Task Update(Session session);

        Task<FileModel> GetById(Guid id);

        Task<List<FileModel>> GetAll(Guid sessionId);
    }
}
