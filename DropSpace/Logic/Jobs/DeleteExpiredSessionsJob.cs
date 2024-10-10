using DropSpace.Infrastructure;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using Quartz;

namespace DropSpace.Logic.Jobs
{
    public class DeleteExpiredSessionsJob(
    ApplicationContext applicationContext,
    ISessionService sessionService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var currentTime = DateTime.UtcNow;

            var sessions = applicationContext.Sessions
                .Where(session =>
                        session.Created + session.Duration < currentTime);

            foreach (var session in sessions)
            {
                await sessionService.Delete(session.Id);
            }
        }
    }
}
