using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Tests.Services
{
    public class PersonServiceTests
    {
        private readonly Mock<IPersonRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PersonService>> _loggerMock;
        private readonly PersonService _service;

        public PersonServiceTests()
        {
            _repositoryMock = new Mock<IPersonRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<PersonService>>();
            _service = new PersonService(
                _repositoryMock.Object, 
                _mapperMock.Object, 
                _loggerMock.Object);
        }

        #region GetALl
        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaPageResponseConDatos()
        {
            // Arrange
            int page = 2;
            int pageSize = 5;
            var persons = new List<Person>
            {
                new Person { Id = 1, FullName = "Juan Pérez", IsActive = true, Cars = new List<Car>() },
                new Person { Id = 2, FullName = "María Gómez", IsActive = true, Cars = new List<Car>() }
            };
            int totalRecords = 10; // Total de personas activas

            var dtos = new List<PersonResponseDto>
            {
                new PersonResponseDto { Id = 1, FullName = "Juan Pérez", IsActive = true, Cars = new List<CarDto>() },
                new PersonResponseDto { Id = 2, FullName = "María Gómez", IsActive = true, Cars = new List<CarDto>() }
            };

            // Configurar el repositorio para que acepte el predicado y los includes
            _repositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync((persons, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<PersonResponseDto>>(persons))
                .Returns(dtos);

            // Act
            var result = await _service.GetAllAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(totalRecords, result.TotalRecords);
            Assert.Equal(dtos, result.Data);

            // Verificar que se llamó al repositorio con los parámetros correctos
            _repositoryMock.Verify(
                r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()),
                Times.Once);

            // Verificar que se llamó al mapper con la lista de personas
            _mapperMock.Verify(m => m.Map<IEnumerable<PersonResponseDto>>(persons), Times.Once);

            // Verificar logs
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Obteniendo personas paginadas: página {page}, tamaño {pageSize}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Se obtuvieron {dtos.Count} personas de un total de {totalRecords}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_SinPersonas_RetornaPageResponseConListaVacia()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var persons = new List<Person>();
            int totalRecords = 0;

            _repositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync((persons, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<PersonResponseDto>>(persons))
                .Returns(new List<PersonResponseDto>());

            // Act
            var result = await _service.GetAllAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(totalRecords, result.TotalRecords);
            Assert.Empty(result.Data);

            // Verificar logs
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se obtuvieron 0 personas de un total de 0")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_VerificaQueSePasaElPredicadoYLosIncludes()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            _repositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync((new List<Person>(), 0));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<PersonResponseDto>>(It.IsAny<IEnumerable<Person>>()))
                .Returns(new List<PersonResponseDto>());

            // Act
            await _service.GetAllAsync(page, pageSize);

            // Assert: verificar que el predicado es p => p.IsActive
            _repositoryMock.Verify(r => r.GetPagedAsync(
                page,
                pageSize,
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { IsActive = true }) == true &&
                    expr.Compile().Invoke(new Person { IsActive = false }) == false),
                It.Is<Expression<Func<Person, object>>>(expr =>
                    expr.Body.ToString().Contains("Cars") || // Verificación simple: el include apunta a Cars
                    expr.Compile().Invoke(new Person { Cars = new List<Car>() }) != null)),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_CuandoRepositorioLanzaExcepcion_PropagaExcepcion()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var expectedException = new InvalidOperationException("Error en base de datos");

            _repositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetAllAsync(page, pageSize));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task GetAllAsync_CuandoMapperLanzaExcepcion_PropagaExcepcion()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var persons = new List<Person> { new Person { Id = 1, FullName = "Test", IsActive = true } };
            int totalRecords = 1;
            var expectedException = new AutoMapperMappingException("Error al mapear");

            _repositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync((persons, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<PersonResponseDto>>(persons))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.GetAllAsync(page, pageSize));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsPersonResponseDto()
        {
            // Arrange
            var createDto = new CreatePersonDto
            {
                NationalId = "12345678",
                FullName = "Juan Pérez"
            };

            var personEntity = new Person
            {
                Id = 1,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true
            };

            var responseDto = new PersonResponseDto
            {
                Id = 1,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true,
                Cars = new List<CarDto>()
            };

            // Configurar mapeo de DTO a entidad
            _mapperMock
                .Setup(m => m.Map<Person>(createDto))
                .Returns(personEntity);

            // Configurar repositorio para que no haga nada (AddAsync probablemente es Task)
            _repositoryMock
                .Setup(r => r.AddAsync(personEntity))
                .Returns(Task.CompletedTask);

            // Configurar mapeo de entidad a response DTO
            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(personEntity))
                .Returns(responseDto);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);

            // Verificar que se llamó al mapper con el DTO de entrada
            _mapperMock.Verify(m => m.Map<Person>(createDto), Times.Once);

            // Verificar que se llamó al repositorio para agregar la entidad
            _repositoryMock.Verify(r => r.AddAsync(personEntity), Times.Once);

            // Verificar que se llamó al mapper para convertir a response DTO
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(personEntity), Times.Once);

            // Verificar logs
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Creando nueva persona con NationalId {createDto.NationalId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona creada con ID {personEntity.Id}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task CreateAsync_CuandoRepositoryLanzaExcepcion_PropagaExcepcion()
        {
            // Arrange
            var createDto = new CreatePersonDto
            {
                NationalId = "12345678",
                FullName = "Juan Pérez"
            };

            var personEntity = new Person
            {
                NationalId = "12345678",
                FullName = "Juan Pérez"
            };

            var expectedException = new InvalidOperationException("Error al guardar en BD");

            _mapperMock
                .Setup(m => m.Map<Person>(createDto))
                .Returns(personEntity);

            _repositoryMock
                .Setup(r => r.AddAsync(personEntity))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal(expectedException.Message, exception.Message);

            // Verificar que se intentó mapear y agregar
            _mapperMock.Verify(m => m.Map<Person>(createDto), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(personEntity), Times.Once);
            // El segundo mapper no debería llamarse porque falla antes
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CuandoMapperLanzaExcepcion_PropagaExcepcion()
        {
            // Arrange
            var createDto = new CreatePersonDto
            {
                NationalId = "12345678",
                FullName = "Juan Pérez"
            };

            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _mapperMock
                .Setup(m => m.Map<Person>(createDto))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal(expectedException.Message, exception.Message);

            // Verificar que no se llamó al repositorio ni al segundo mapeo
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Never);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CuandoNationalIdYaExiste_LanzaExcepcion()
        {
            // Arrange
            var createDto = new CreatePersonDto
            {
                NationalId = "12345678", // NationalId que ya existe en la BD
                FullName = "Juan Pérez"
            };

            var personEntity = new Person
            {
                NationalId = "12345678",
                FullName = "Juan Pérez"
            };

            // Simular una excepción de duplicado (puede ser DbUpdateException o cualquier otra)
            var duplicateException = new InvalidOperationException("Ya existe una persona con este NationalId");

            _mapperMock
                .Setup(m => m.Map<Person>(createDto))
                .Returns(personEntity);

            _repositoryMock
                .Setup(r => r.AddAsync(personEntity))
                .ThrowsAsync(duplicateException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal("Ya existe una persona con este NationalId", exception.Message);

            // Verificar que se intentó mapear y agregar
            _mapperMock.Verify(m => m.Map<Person>(createDto), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(personEntity), Times.Once);
            // El segundo mapeo no debería ejecutarse porque falló antes
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);
        }
        #endregion

        #region UpdateAsync
        // 1. Actualización exitosa
        [Fact]
        public async Task UpdateAsync_ExistingPerson_UpdatesAndReturnsPersonResponseDto()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez Actualizado" };
            var existingPerson = new Person
            {
                Id = id,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true
            };
            var updatedPerson = new Person
            {
                Id = id,
                NationalId = "12345678",
                FullName = "Juan Pérez Actualizado",
                IsActive = true
            };
            var responseDto = new PersonResponseDto
            {
                Id = id,
                NationalId = "12345678",
                FullName = "Juan Pérez Actualizado",
                IsActive = true,
                Cars = new List<CarDto>()
            };

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            // Configurar el mapper para actualizar la entidad (puede ser void o devolver la entidad)
            _mapperMock
                .Setup(m => m.Map(updateDto, existingPerson))
                .Callback<UpdatePersonDto, Person>((dto, entity) => { entity.FullName = dto.FullName; })
                .Returns(updatedPerson); // Asumiendo que devuelve la entidad modificada

            _repositoryMock
                .Setup(r => r.UpdateAsync(existingPerson))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(existingPerson))
                .Returns(responseDto);

            // Act
            var result = await _service.UpdateAsync(id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);

            // Verificaciones
            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { Id = id }) == true &&
                    expr.Compile().Invoke(new Person { Id = id + 1 }) == false)),
                Times.Once);

            _mapperMock.Verify(m => m.Map(updateDto, existingPerson), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(existingPerson), Times.Once);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(existingPerson), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Actualizando persona con ID {id}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con ID {id} actualizada correctamente")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // 2. Persona no encontrada → lanza NotFoundException
        [Fact]
        public async Task UpdateAsync_PersonNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int id = 999;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez" };

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Person)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(id, updateDto));

            Assert.Equal("Persona no encontrada", exception.Message);

            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { Id = id }) == true)),
                Times.Once);

            // No se debe llamar a más operaciones
            _mapperMock.Verify(m => m.Map(It.IsAny<UpdatePersonDto>(), It.IsAny<Person>()), Times.Never);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);

            // Log de warning
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con ID {id} no encontrada para actualizar")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // 3. Excepción en GetFirstOrDefaultAsync
        [Fact]
        public async Task UpdateAsync_WhenGetFirstOrDefaultThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez" };
            var expectedException = new InvalidOperationException("Error de BD al buscar");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(id, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        // 4. Excepción en Map (DTO → entidad)
        [Fact]
        public async Task UpdateAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez" };
            var existingPerson = new Person { Id = id };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingPerson))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.UpdateAsync(id, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);

            // No debe llegar a UpdateAsync ni al segundo mapper
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);
        }

        // 5. Excepción en UpdateAsync
        [Fact]
        public async Task UpdateAsync_WhenUpdateThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez" };
            var existingPerson = new Person { Id = id };
            var expectedException = new InvalidOperationException("Error de BD al actualizar");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingPerson))
                .Returns(existingPerson); // o .Callback(...)

            _repositoryMock
                .Setup(r => r.UpdateAsync(existingPerson))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(id, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        // 6. Excepción en Map (entidad → DTO de respuesta)
        [Fact]
        public async Task UpdateAsync_WhenResponseMapperThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdatePersonDto { FullName = "Juan Pérez" };
            var existingPerson = new Person { Id = id };
            var expectedException = new AutoMapperMappingException("Error al mapear a response");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingPerson))
                .Returns(existingPerson);

            _repositoryMock
                .Setup(r => r.UpdateAsync(existingPerson))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(existingPerson))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.UpdateAsync(id, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        #endregion

        #region GetByNationalIdAsync
        [Fact]
        public async Task GetByNationalIdAsync_ExistingNationalId_ReturnsPersonResponseDto()
        {
            // Arrange
            string nationalId = "12345678";
            var personEntity = new Person
            {
                Id = 1,
                NationalId = nationalId,
                FullName = "Juan Pérez",
                IsActive = true,
                Cars = new List<Car> { new Car { Id = 1, Model = "Toyota" } }
            };
            var responseDto = new PersonResponseDto
            {
                Id = 1,
                NationalId = nationalId,
                FullName = "Juan Pérez",
                IsActive = true,
                Cars = new List<CarDto> { new CarDto { Id = 1, Model = "Toyota" } }
            };

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync(personEntity);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(personEntity))
                .Returns(responseDto);

            // Act
            var result = await _service.GetByNationalIdAsync(nationalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);

            // Verificar que se llamó al repositorio con el predicado correcto y el include
            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { NationalId = nationalId }) == true &&
                    expr.Compile().Invoke(new Person { NationalId = "otro" }) == false),
                It.Is<Expression<Func<Person, object>>>(expr =>
                    expr.Body.ToString().Contains("Cars"))),
                Times.Once);

            _mapperMock.Verify(m => m.Map<PersonResponseDto>(personEntity), Times.Once);

            // Verificar logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Buscando persona por NationalId {nationalId}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con NationalId {nationalId} encontrada (ID {personEntity.Id})")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByNationalIdAsync_NonExistingNationalId_ThrowsNotFoundException()
        {
            // Arrange
            string nationalId = "99999999";

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync((Person)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetByNationalIdAsync(nationalId));

            Assert.Equal("Persona no encontrada", exception.Message);

            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { NationalId = nationalId }) == true),
                It.IsAny<Expression<Func<Person, object>>>()),
                Times.Once);

            // No debe llamarse al mapper
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);

            // Verificar log de warning
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con NationalId {nationalId} no encontrada")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByNationalIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            string nationalId = "12345678";
            var expectedException = new InvalidOperationException("Error de base de datos");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetByNationalIdAsync(nationalId));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task GetByNationalIdAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            string nationalId = "12345678";
            var personEntity = new Person
            {
                Id = 1,
                NationalId = nationalId,
                FullName = "Juan Pérez"
            };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>>()))
                .ReturnsAsync(personEntity);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(personEntity))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.GetByNationalIdAsync(nationalId));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region ToggleActive
        [Fact]
        public async Task ToggleActive_ExistingPerson_FromActiveToInactive_TogglesAndReturnsDto()
        {
            // Arrange
            int id = 1;
            var existingPerson = new Person
            {
                Id = id,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true
            };
            var responseDto = new PersonResponseDto
            {
                Id = id,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = false,
                Cars = new List<CarDto>()
            };

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Person>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(It.IsAny<Person>()))
                .Returns(responseDto);

            // Act
            var result = await _service.ToggleActive(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            Assert.False(existingPerson.IsActive); // Verificar que se cambió en la entidad

            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Person>(p => p.Id == id && p.IsActive == false)), Times.Once);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(existingPerson), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cambiando estado activo de persona con ID {id}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con ID {id} ahora está inactivo")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_ExistingPerson_FromInactiveToActive_TogglesAndReturnsDto()
        {
            // Arrange
            int id = 2;
            var existingPerson = new Person
            {
                Id = id,
                NationalId = "87654321",
                FullName = "María Gómez",
                IsActive = false
            };
            var responseDto = new PersonResponseDto
            {
                Id = id,
                NationalId = "87654321",
                FullName = "María Gómez",
                IsActive = true,
                Cars = new List<CarDto>()
            };

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Person>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(It.IsAny<Person>()))
                .Returns(responseDto);

            // Act
            var result = await _service.ToggleActive(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            Assert.True(existingPerson.IsActive);

            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Person>(p => p.Id == id && p.IsActive == true)), Times.Once);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(existingPerson), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cambiando estado activo de persona con ID {id}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con ID {id} ahora está activo")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_PersonNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int id = 999;

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Person)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.ToggleActive(id));

            Assert.Equal("person not found", exception.Message);

            _repositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { Id = id }) == true)),
                Times.Once);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never);
            _mapperMock.Verify(m => m.Map<PersonResponseDto>(It.IsAny<Person>()), Times.Never);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Persona con ID {id} no encontrada para cambiar estado")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_WhenGetFirstOrDefaultThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var expectedException = new InvalidOperationException("Error de BD al buscar");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ToggleActive(id));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task ToggleActive_WhenUpdateThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var existingPerson = new Person { Id = id, IsActive = true };
            var expectedException = new InvalidOperationException("Error de BD al actualizar");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _repositoryMock
                .Setup(r => r.UpdateAsync(existingPerson))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ToggleActive(id));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task ToggleActive_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            int id = 1;
            var existingPerson = new Person { Id = id, IsActive = true };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _repositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(existingPerson);

            _repositoryMock
                .Setup(r => r.UpdateAsync(existingPerson))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<PersonResponseDto>(existingPerson))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.ToggleActive(id));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion
    }
}

