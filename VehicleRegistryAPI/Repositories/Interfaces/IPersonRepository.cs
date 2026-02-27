using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Generics;

namespace VehicleRegistryAPI.Repositories.Interfaces
{
    public interface IPersonRepository : IGenericRepository<Person>
    {
        Task<Person?> GetByNationalIdAsync(string nationalId);
    }
}
