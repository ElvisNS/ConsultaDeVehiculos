using AutoMapper;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
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
        }
    }
}
