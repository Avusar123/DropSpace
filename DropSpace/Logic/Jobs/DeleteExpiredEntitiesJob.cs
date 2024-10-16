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
        }
    }
}
