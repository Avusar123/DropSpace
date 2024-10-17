using DropSpace.Infrastructure;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DropSpace.Logic.Jobs
{
    public class DeleteDeadUploadsJob(
        ApplicationContext applicationContext, 
        IUploadStateStore uploadStateStore,
        IFileService fileService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var fileIds = await applicationContext.Files
                .Where(file => !file.IsUploaded)
                .Select(file => file.Id)
                .ToListAsync();

            foreach (var fileId in fileIds)
            {
                if ((await uploadStateStore.GetByFileId(fileId)) == null)
                {
                    await fileService.Delete(fileId);
                }
            }
        }
    }
}
