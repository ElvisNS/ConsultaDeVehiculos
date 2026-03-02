using System.Linq.Expressions;
using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Repositories.Generics
{
    public interface IGenericRepository<TEntity>
    where TEntity : BaseEntity
    {
        Task<(IEnumerable<TEntity> Data, int TotalRecords)> GetPagedAsync(
            int page,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes);

        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        //Task DeleteAsync(TEntity entity);

    }
}
