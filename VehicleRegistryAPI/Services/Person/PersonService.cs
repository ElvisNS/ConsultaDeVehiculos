using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Services.Person
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PersonService> _logger;

        public PersonService(
            IPersonRepository personRepository,
            IMapper mapper,
            ILogger<PersonService> logger)
        {
            _personRepository = personRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PageResponse<PersonResponseDto>> GetAllAsync(int page, int pageSize)
        {
            _logger.LogInformation("Obteniendo personas paginadas: página {Page}, tamaño {PageSize}", page, pageSize);
            var (persons, totalRecords) = await _personRepository
                .GetPagedAsync(page, pageSize, p => p.IsActive, p => p.Cars); 

            var mappedPersons = _mapper.Map<IEnumerable<PersonResponseDto>>(persons);

            _logger.LogInformation("Se obtuvieron {Count} personas de un total de {TotalRecords}", mappedPersons.Count(), totalRecords);

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
            _logger.LogInformation("Creando nueva persona con NationalId {NationalId}", dto.NationalId);

            var person = _mapper.Map<VehicleRegistryAPI.Entities.Person>(dto);

            await _personRepository.AddAsync(person);

            _logger.LogInformation("Persona creada con ID {PersonId}", person.Id);

            return _mapper.Map<PersonResponseDto>(person);
        }
        public async Task<PersonResponseDto> UpdateAsync(int id, UpdatePersonDto dto)
        {
            _logger.LogInformation("Actualizando persona con ID {PersonId}", id);
            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                _logger.LogWarning("Persona con ID {PersonId} no encontrada para actualizar", id);
                throw new NotFoundException("Persona no encontrada");
            }

            _mapper.Map(dto, person);

            await _personRepository.UpdateAsync(person);

            _logger.LogInformation("Persona con ID {PersonId} actualizada correctamente", id);

            return _mapper.Map<PersonResponseDto>(person);

        }
        public async Task<PersonResponseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando persona por ID {PersonId}", id);

            var person = await _personRepository
                .GetFirstOrDefaultAsync(
                p => p.Id == id,
                c => c.Cars
                );

            if (person == null)
            {
                _logger.LogWarning("Persona con ID {PersonId} no encontrada", id);
                throw new NotFoundException("Persona no encontrada");
            }

            _logger.LogInformation("Persona con ID {PersonId} encontrada", id);

            return _mapper.Map<PersonResponseDto>(person);
        }
        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            _logger.LogDebug("Verificando existencia de persona con NationalId {NationalId}", nationalId);

            var exists = await _personRepository.AnyAsync(p => p.NationalId == nationalId);

            _logger.LogDebug("Resultado de existencia para NationalId {NationalId}: {Exists}", nationalId, exists);

            return exists;
        }
        public async Task<PersonResponseDto> GetByNationalIdAsync(string nationalId)
        {
            _logger.LogInformation("Buscando persona por NationalId {NationalId}", nationalId);

            var person = await _personRepository
                .GetFirstOrDefaultAsync(
                    p => p.NationalId == nationalId,
                    c => c.Cars
                );

            if (person == null)
            {
                _logger.LogWarning("Persona con NationalId {NationalId} no encontrada", nationalId);
                throw new NotFoundException("Persona no encontrada");
            }

            _logger.LogInformation("Persona con NationalId {NationalId} encontrada (ID {PersonId})", nationalId, person.Id);
            return _mapper.Map<PersonResponseDto>(person);
        }
        public async Task<PersonResponseDto> ToggleActive(int id)
        {
            _logger.LogInformation("Cambiando estado activo de persona con ID {PersonId}", id);

            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                _logger.LogWarning("Persona con ID {PersonId} no encontrada para cambiar estado", id);
                throw new NotFoundException("person not found");
            }

            person.IsActive = !person.IsActive;
            var nuevoEstado = person.IsActive ? "activo" : "inactivo";

            await _personRepository.UpdateAsync(person);

            _logger.LogInformation("Persona con ID {PersonId} ahora está {Estado}", id, nuevoEstado);

            return _mapper.Map<PersonResponseDto>(person);
        }
    }
}