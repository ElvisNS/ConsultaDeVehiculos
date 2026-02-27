using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Generics;

namespace VehicleRegistryAPI.Repositories.Interfaces
{
    public interface ICarRepository : IGenericRepository<Car>
    {
        Task<Car?> GetByPlateNumberAsync(string plateNumber);
    }
}
