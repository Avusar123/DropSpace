using DropSpace.Domain;

namespace DropSpace.Infrastructure.Stores.Interfaces
{
    public interface IFileStore
    {
        ApplicationContext ApplicationContext { get; }

        Task<FileModel> CreateAsync(FileModel fileModel);

        Task Delete(Guid id);

        Task Update(FileModel fileModel);

        Task<FileModel> GetById(Guid id);

        Task<List<FileModel>> GetAll(Guid sessionId);
    }
}
