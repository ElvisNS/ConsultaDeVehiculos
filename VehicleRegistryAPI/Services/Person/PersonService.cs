using AutoMapper;
using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Services.Person
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public PersonService(
            IPersonRepository personRepository,
            IMapper mapper)
        {
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<PageResponse<PersonResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var (persons, totalRecords) = await _personRepository
                .GetPagedAsync(page, pageSize, p => p.Cars);

            var mappedPersons = _mapper.Map<IEnumerable<PersonResponseDto>>(persons);

            return new PageResponse<PersonResponseDto>
            {
                Data = mappedPersons,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PersonResponseDto> CreateAsync(CreatePersonDto dto)
        {
            var person = _mapper.Map<VehicleRegistryAPI.Entities.Person>(dto);

            await _personRepository.AddAsync(person);

            return _mapper.Map<PersonResponseDto>(person);
        }

        public async Task<PersonResponseDto> UpdateAsync(int id, UpdatePersonDto dto)
        {
            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
                throw new NotFoundException("Persona no encontrada");

            _mapper.Map(dto, person);

            await _personRepository.UpdateAsync(person);

            return _mapper.Map<PersonResponseDto>(person);
        }

        public async Task<PersonResponseDto> GetByIdAsync(int id)
        {
            var person = await _personRepository
                .GetFirstOrDefaultAsync(
                p => p.Id == id,
                c => c.Cars
                );

            if (person == null)
                throw new NotFoundException("Persona no encontrada");

            return _mapper.Map<PersonResponseDto>(person);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            return await _personRepository.AnyAsync(p => p.NationalId == nationalId);
        }

        public async Task<PersonResponseDto> GetByNationalIdAsync(string nationalId)
        {
            var person = await _personRepository
                .GetFirstOrDefaultAsync(
                p=> p.NationalId ==  nationalId,
                c => c.Cars
                );

            if (person == null)
                throw new NotFoundException("Persona no encontrada");

            return _mapper.Map<PersonResponseDto>(person);
        }

    }
}

