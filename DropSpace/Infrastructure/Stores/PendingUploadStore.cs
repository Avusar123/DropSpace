using DropSpace.Domain;
using DropSpace.Infrastructure.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Infrastructure.Stores
{
    public class PendingUploadStore(ApplicationContext applicationContext) : IPendingUploadStore
    {
        public async Task<Guid> CreateAsync(PendingUploadModel model)
        {
            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            model.LastChunkUploaded = DateTime.UtcNow;

            applicationContext.PendingUploads.Add(model);

            await applicationContext.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var upload = applicationContext.PendingUploads.FirstOrDefault(x => x.Id == id);

            if (upload != null)
            {
                applicationContext.Remove(upload);
            }

            await applicationContext.SaveChangesAsync();
        }

        public async Task<PendingUploadModel> GetById(Guid id)
        {
            return await applicationContext
                .PendingUploads
                .Include(upload => upload.File)
                .ThenInclude(upload => upload.Session)
                .Where(upload => upload.Id == id && upload.IsCompleted == false)
                .FirstOrDefaultAsync() ?? throw new NullReferenceException("Загрузка не найдена");
        }

        public async Task UpdateAsync(PendingUploadModel model)
        {
            applicationContext.Update(model);

            await applicationContext.SaveChangesAsync();
        }
    }
}
