using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Services;
using DropSpace.Services.Interfaces;
using Quartz;

namespace DropSpace.Jobs
{
    public class DeleteExpiredSessionsJob(
    ApplicationContext applicationContext,
    ISessionService sessionService,
    IEventTransmitter eventTransmitter) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var sessions = applicationContext.Sessions
                .Where(session =>
                        session.Created + session.Duration < DateTime.Now);

            foreach (var session in sessions)
            {
                await sessionService.Delete(session.Id);

                applicationContext.Remove(session);
            }

            await applicationContext.SaveChangesAsync();
        }
    }
}
