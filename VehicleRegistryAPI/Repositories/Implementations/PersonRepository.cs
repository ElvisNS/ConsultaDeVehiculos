using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Generics;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Repositories.Implementations
{
    public class PersonRepository : GenericRepository<Person>, IPersonRepository
    {
        private readonly ApplicationDbContext _dbcontext;
        public PersonRepository(ApplicationDbContext context) : base(context)
        {
            _dbcontext = context;
        }

        public async Task<Person?> GetByNationalIdAsync(string nationalId)
        {
            return await _dbcontext.Persons
                .FirstOrDefaultAsync(p => p.NationalId == nationalId);
        }
    }
}
