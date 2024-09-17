using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Files.Interfaces;
using DropSpace.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DropSpace.Jobs
{
public class DeleteTimeoutUploadsJob(
    ApplicationContext applicationContext, 
    IEventTransmitter eventTransmitter,
    IFileVault fileSaver,
    IConfiguration configuration) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var uploads = applicationContext.PendingUploads
                .Include(upload => upload.Session)
                    .ThenInclude(upload => upload.Members)
                .Where(upload => 
                        DateTime.Now - upload.LastChunkUploaded >= 
                            TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs")));

            foreach (var upload in uploads)
            {
                applicationContext.PendingUploads.Remove(upload);

                await applicationContext.SaveChangesAsync();

                await eventTransmitter.FireEvent(new FileListChangedEvent() 
                { 
                    UserIds = 
                        upload.
                        Session
                        .Members
                        .Select(m => m.UserId)
                        .ToList()
                });
            }
        }
    }
}
