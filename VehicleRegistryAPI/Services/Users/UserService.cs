using AutoMapper;
using System.ComponentModel;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Exceptions;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> AddUser(CreateUserDto dto) 
        {
            var user = _mapper.Map<User>(dto);

            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);

            await _userRepository.AddAsync(user);

            var userResponse = _mapper.Map<UserResponseDto>(user);

            return userResponse;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto> GetById(int id)
        {
            var users = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserResponseDto>(users);
        }

        public async Task<UserResponseDto> UpdateUser(int id, UpdateUserDto dto)
        {
            var users = await _userRepository.GetByIdAsync(id);
            if (users == null)
                throw new NotFoundException("Usuario no encontrado");

            _mapper.Map(dto, users);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                users.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            }

            await _userRepository.UpdateAsync(users);

            return _mapper.Map<UserResponseDto>(users);
        }

        public async Task<UserResponseDto> ToggleActiveUser(int id)
        {
            var users = await _userRepository.GetByIdAsync(id);
            if (users == null)
                throw new NotFoundException("Usuario no encontrado");

            users.IsActive = !users.IsActive;
            await _userRepository.UpdateAsync(users);
            return _mapper.Map<UserResponseDto>(users);
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _userRepository.ExistsByEmailAsync(email);
        }

    }
}
