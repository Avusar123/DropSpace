namespace DropSpace.Services
{
    public interface IService<TEntity, TKey>
    {
        public Task<TKey> CreateAsync(TEntity entity);

        public Task Update(TEntity entity);

        public Task Delete(TKey key);

        public Task<TEntity> GetAsync(TKey key);
    }
}
