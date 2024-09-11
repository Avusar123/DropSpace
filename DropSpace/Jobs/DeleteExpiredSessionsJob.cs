using DropSpace.Events.Events;
using DropSpace.Events.Interfaces;
using DropSpace.Services;
using Quartz;

namespace DropSpace.Jobs
{
public class DeleteExpiredSessionsJob(
    ApplicationContext applicationContext, 
    IEventTransmitter eventTransmitter) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var sessions = applicationContext.Sessions
                .Where(session => 
                        session.Created + session.Duration < DateTime.Now);

            foreach (var session in sessions)
            {
                await eventTransmitter.FireEvent(new SessionExpiredEvent() 
                { 
                    UserIds = 
                        session
                        .Members
                        .Select(m => m.UserId)
                        .ToList()
                });

                applicationContext.Remove(session);
            }

            await applicationContext.SaveChangesAsync();
        }
    }
}
