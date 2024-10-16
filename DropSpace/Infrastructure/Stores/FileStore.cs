using DropSpace.Contracts.Dtos;
using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Infrastructure.Stores
{
    public class FileStore(
        ApplicationContext applicationContext) : IFileStore
    {
        public ApplicationContext ApplicationContext { get; } = applicationContext;

        public async Task<FileModel> CreateAsync(FileModel fileModel)
        {
            if (fileModel.Id == Guid.Empty)
                fileModel.Id = Guid.NewGuid();

            ApplicationContext.Files.Add(fileModel);

            await ApplicationContext.SaveChangesAsync();

            return fileModel;
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
                .Where(file => file.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<FileModel> GetById(Guid id)
        {
            return await ApplicationContext
                                    .Files
                                    .Include(file => file.Session)
                                        .ThenInclude(session => session.Members)
                                    .Where(file => file.Id == id)
                                    .FirstOrDefaultAsync()
                                        ?? throw new NullReferenceException("Файл не найден!");
        }

        public async Task Update(FileModel file)
        {
            ApplicationContext.Update(file);

            await ApplicationContext.SaveChangesAsync();
        }
    }
}
