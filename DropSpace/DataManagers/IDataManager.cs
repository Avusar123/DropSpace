namespace DropSpace.DataManagers
{
    public interface IDataManager<EntityT, KeyT>
    {
        public Task<EntityT> GetAsync(KeyT key);

        public Task<EntityT> CreateAsync(EntityT entity);

        public Task UpdateAsync(EntityT entity);

        public Task DeleteAsync(KeyT key);
    }
}
