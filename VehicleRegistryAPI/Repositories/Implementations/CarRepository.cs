using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Generics;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Repositories.Implementations
{
    public class CarRepository : GenericRepository<Car>, ICarRepository
    {

        private readonly ApplicationDbContext _dbcontext;
        public CarRepository(ApplicationDbContext context) : base(context)
        {
            _dbcontext = context;
        }

        public async Task<Car?> GetByPlateNumberAsync(string plateNumber)
        {
            return await _dbcontext.Cars
                .FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
        }
    }
}
