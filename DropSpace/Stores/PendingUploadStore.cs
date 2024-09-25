using DropSpace.Models.Data;
using DropSpace.Stores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropSpace.Stores
{
    public class PendingUploadStore(ApplicationContext applicationContext) : IPendingUploadStore
    {
        public async Task<Guid> CreateAsync(PendingUploadModel model)
        {
            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            model.LastChunkUploaded = DateTime.Now;

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
                .Where(upload => upload.Id == id)
                .Include(upload => upload.Session)
                .FirstOrDefaultAsync() ?? throw new NullReferenceException("Загрузка не найдена");
        }

        public async Task<List<PendingUploadModel>> GetAll(Guid sessionId)
        {
            return await applicationContext.PendingUploads
                .Where(upload => upload.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task UpdateAsync(PendingUploadModel model)
        {
            applicationContext.Update(model);

            await applicationContext.SaveChangesAsync();
        }
    }
}
