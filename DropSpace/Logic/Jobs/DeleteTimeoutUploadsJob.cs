using DropSpace.Infrastructure;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Files.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DropSpace.Logic.Jobs
{
    public class DeleteTimeoutUploadsJob(
    ApplicationContext applicationContext,
    IEventTransmitter eventTransmitter,
    IFileVault fileVault,
    IConfiguration configuration) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var uploadTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs"));

            var expiredTime = DateTime.Now.Add(uploadTimeout);
            
            var uploads = applicationContext.PendingUploads
                .Include(upload => upload.File)
                    .ThenInclude(file => file.Session)
                    .ThenInclude(session => session.Members)
                .Where(upload =>
                        upload.LastChunkUploaded >= expiredTime && !upload.IsCompleted);

            foreach (var upload in uploads)
            {
                using (var transaction = await applicationContext.Database.BeginTransactionAsync())
                {
                    applicationContext.PendingUploads.Remove(upload);

                    applicationContext.Files.Remove(upload.File);

                    await fileVault.DeleteAsync(upload.File.Id.ToString());

                    await applicationContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }

                //await eventTransmitter.FireEvent(new NewCh()
                //{
                //    UserIds =
                //        upload.
                //        Session
                //        .Members
                //        .Select(m => m.UserId)
                //        .ToList()
                //});
            }
        }
    }
}
