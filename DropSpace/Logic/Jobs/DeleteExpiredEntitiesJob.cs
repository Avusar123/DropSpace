using DropSpace.Infrastructure;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Files;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Configuration;

namespace DropSpace.Logic.Jobs
{
    public class DeleteExpiredEntitiesJob(
    ApplicationContext applicationContext,
    ISessionService sessionService,
    IConfiguration configuration,
    IFileVault fileVault) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var currentTime = DateTime.UtcNow;

            var sessions = applicationContext.Sessions
                .Where(session =>
                        session.Created + session.Duration < currentTime)
                .AsNoTracking()
                .ToList();

            foreach (var session in sessions)
            {
                await sessionService.Delete(session.Id);
            }

            var uploadTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs"));

            var uploads = applicationContext.PendingUploads
                .Include(upload => upload.File)
                    .ThenInclude(file => file.Session)
                    .ThenInclude(session => session.Members)
                .Where(upload =>
                        DateTime.UtcNow - upload.LastChunkUploaded >= uploadTimeout && !upload.IsCompleted)
                .AsNoTracking()
                .ToList();

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
