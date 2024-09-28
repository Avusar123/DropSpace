using DropSpace.Models.Data;
using DropSpace.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Stores
{
    public class FileStore(
        ApplicationContext applicationContext) : IFileStore
    {
        public async Task<Guid> CreateAsync(FileModel fileModel)
        {
            if (fileModel.Id == Guid.Empty)
                fileModel.Id = Guid.NewGuid();

            applicationContext.Files.Add(fileModel);

            await applicationContext.SaveChangesAsync();

            return fileModel.Id;
        }

        public async Task Delete(Guid id)
        {
            var file = applicationContext.Files.SingleOrDefault(f => f.Id == id);

            if (file != null)
            {
                applicationContext.Files.Remove(file);
            }

            await applicationContext.SaveChangesAsync();
        }

        public async Task<List<FileModel>> GetAll(Guid sessionId)
        {
            return await applicationContext.Files
                .Include(file => file.PendingUpload)
                .Where(file => file.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<FileModel> GetById(Guid id)
        {
            return await applicationContext
                                    .Files
                                    .Include(file => file.PendingUpload)
                                    .Where(file => file.Id == id)
                                    .FirstOrDefaultAsync()
                                        ?? throw new NullReferenceException("Файл не найден!");
        }

        public async Task Update(Session session)
        {
            applicationContext.Update(session);

            await applicationContext.SaveChangesAsync();
        }
    }
}
