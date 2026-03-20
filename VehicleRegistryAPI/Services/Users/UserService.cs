using AutoMapper;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Users;
using VehicleRegistryAPI.Tools.Exceptions;
using VehicleRegistryAPI.Tools.Security;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger; 

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger) 
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserResponseDto> AddUser(CreateUserDto dto)
    {
        // Log de inicio
        _logger.LogInformation("Intentando crear usuario con email {Email}", dto.Email);

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _passwordHasher.HashPassword(dto.Password);

        await _userRepository.AddAsync(user);

        // Log de éxito
        _logger.LogInformation("Usuario creado con ID {UserId} y email {Email}", user.Id, user.Email);

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAll()
    {
        _logger.LogDebug("Obteniendo todos los usuarios"); 

        var users = await _userRepository.GetAllAsync();

        _logger.LogDebug("Se obtuvieron {Count} usuarios", users.Count());

        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto> GetById(int id)
    {
        _logger.LogInformation("Buscando usuario con ID {UserId}", id);
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado", id);
            throw new NotFoundException("Usuario no encontrado");
        }

        _logger.LogInformation("Usuario {UserId} encontrado", id);
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> UpdateUser(int id, UpdateUserDto dto)
    {
        _logger.LogInformation("Actualizando usuario ID {UserId}", id);
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Intento de actualizar usuario inexistente ID {UserId}", id);
            throw new NotFoundException("Usuario no encontrado");
        }

        _mapper.Map(dto, user);

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
            _logger.LogDebug("Contraseña actualizada para usuario {UserId}", id);
        }

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Usuario {UserId} actualizado correctamente", id);

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> ToggleActiveUser(int id)
    {
        _logger.LogInformation("Cambiando estado activo del usuario {UserId}", id);
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Intento de cambiar estado de usuario inexistente ID {UserId}", id);
            throw new NotFoundException("Usuario no encontrado");
        }

        user.IsActive = !user.IsActive;
        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Usuario {UserId} ahora está {Estado}", id, user.IsActive ? "activo" : "inactivo");

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<bool> EmailExists(string email)
    {
        _logger.LogDebug("Verificando existencia de email {Email}", email);
        var exists = await _userRepository.ExistsByEmailAsync(email);
        _logger.LogDebug("Email {Email} existe: {Exists}", email, exists);
        return exists;
    }
}