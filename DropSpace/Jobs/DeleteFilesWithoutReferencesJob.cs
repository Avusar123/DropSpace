using DropSpace.Files.Interfaces;
using Quartz;

namespace DropSpace.Jobs
{
    public class DeleteFilesWithoutReferencesJob(
        IFileSaver fileSaver,
        ApplicationContext applicationContext,
        IConfiguration configuration) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var hashList = await fileSaver.GetAllFilesHash();

            var uploadExpiredTimeSpan = TimeSpan.FromSeconds(configuration.GetValue<int>("UploadTimeOutSecs"));

            foreach (var hash in hashList)
            {
                if (!applicationContext.Files.Any(file => file.FileHash == hash) &&
                    !applicationContext.PendingUploads.Any(file => file.FileHash == hash) &&
                    (await fileSaver.GetFileCreatedTime(hash)).Add(uploadExpiredTimeSpan) <= DateTime.Now)
                {
                    await fileSaver.DeleteAsync(hash);
                }
            }
        }
    }
}
