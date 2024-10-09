using DropSpace.Infrastructure;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Services.Interfaces;
using Quartz;

namespace DropSpace.Logic.Jobs
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
            }
        }
    }
}
