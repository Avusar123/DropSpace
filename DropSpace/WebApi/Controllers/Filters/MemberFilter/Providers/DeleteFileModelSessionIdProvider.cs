using DropSpace.Contracts.Models;

namespace DropSpace.WebApi.Controllers.Filters.MemberFilter.Providers
{
    public class DeleteFileModelSessionIdProvider : ISessionIdProvider<DeleteFileModel>
    {
        public Guid GetFrom(DeleteFileModel obj)
        {
            return obj.SessionId;
        }
    }
}
