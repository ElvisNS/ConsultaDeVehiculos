using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Repositories.Generics
{
    public interface IGenericRepository<TEntity>
    where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);

    }
}
