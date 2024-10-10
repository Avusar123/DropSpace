using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Infrastructure.Stores
{
    public class FileStore(
        ApplicationContext applicationContext) : IFileStore
    {
        public ApplicationContext ApplicationContext { get; } = applicationContext;

        public async Task<Guid> CreateAsync(FileModel fileModel)
        {
            if (fileModel.Id == Guid.Empty)
                fileModel.Id = Guid.NewGuid();

            ApplicationContext.Files.Add(fileModel);

            await ApplicationContext.SaveChangesAsync();

            return fileModel.Id;
        }

        public async Task Delete(Guid id)
        {
            var file = ApplicationContext.Files.SingleOrDefault(f => f.Id == id);

            if (file != null)
            {
                ApplicationContext.Files.Remove(file);
            }

            await ApplicationContext.SaveChangesAsync();
        }

        public async Task<List<FileModel>> GetAll(Guid sessionId)
        {
            return await ApplicationContext.Files
                .Include(file => file.PendingUpload)
                .Where(file => file.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<FileModel> GetById(Guid id)
        {
            return await ApplicationContext
                                    .Files
                                    .Include(file => file.PendingUpload)
                                    .Include(file => file.Session)
                                    .Where(file => file.Id == id)
                                    .FirstOrDefaultAsync()
                                        ?? throw new NullReferenceException("Файл не найден!");
        }

        public async Task Update(Session session)
        {
            ApplicationContext.Update(session);

            await ApplicationContext.SaveChangesAsync();
        }
    }
}
