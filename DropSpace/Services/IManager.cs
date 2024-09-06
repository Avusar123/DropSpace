namespace DropSpace.Services
{
    public interface IManager<TEntity, TKey>
    {
        public Task<TKey> CreateAsync(TEntity entity);

        public Task Update(TEntity entity);

        public Task Delete(TKey key);

        public Task<TEntity> GetAsync(TKey key);
    }
}
