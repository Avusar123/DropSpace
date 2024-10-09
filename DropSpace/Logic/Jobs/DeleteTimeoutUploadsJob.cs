﻿using DropSpace.Infrastructure;
using DropSpace.Logic.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DropSpace.Logic.Jobs
{
    public class DeleteTimeoutUploadsJob(
    ApplicationContext applicationContext,
    IEventTransmitter eventTransmitter,
    IConfiguration configuration) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var uploads = applicationContext.PendingUploads
                .Include(upload => upload.File)
                    .ThenInclude(file => file.Session)
                    .ThenInclude(session => session.Members)
                .Where(upload =>
                        DateTime.Now - upload.LastChunkUploaded >=
                            TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs")) && !upload.IsCompleted);

            foreach (var upload in uploads)
            {
                applicationContext.PendingUploads.Remove(upload);

                applicationContext.Files.Remove(upload.File);

                await applicationContext.SaveChangesAsync();

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
