using AutoMapper;
using System.Data;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.DTOS.Roles;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            // Person
            CreateMap<Person, PersonResponseDto>();
            CreateMap<CreatePersonDto, Person>();
            CreateMap<UpdatePersonDto, Person>();


            // Car
            CreateMap<Car, CarDto>();
            CreateMap<Car, CarResponseDto>()
                .ForMember(dest => dest.Cedula,
                    opt => opt.MapFrom(src => src.Persons.NationalId))
                .ForMember(dest => dest.Nombre,
                    opt => opt.MapFrom(src => src.Persons.FullName));

            CreateMap<CreateCarDto, Car>();
            CreateMap<UpdateCarDto, Car>();


            CreateMap<Roles, RoleDto>();

            // User
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<User, UserResponseDto>()
                 .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(src =>
                        src.UserRoless.Select(ur => ur.Role))); // Aquí la magia
        }
    }
}
