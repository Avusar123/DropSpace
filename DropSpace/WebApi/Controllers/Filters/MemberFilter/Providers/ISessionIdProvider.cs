namespace DropSpace.WebApi.Controllers.Filters.MemberFilter.Providers
{
    public interface ISessionIdProvider<T>
    {
        public Guid GetFrom(T obj);
    }
}
