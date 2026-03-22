using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Tests.Services
{
    public class CarServiceTests
    {
        private readonly Mock<ICarRepository> _carRepositoryMock;
        private readonly Mock<IPersonRepository> _personRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CarService>> _loggerMock;
        private readonly CarService _service;

        public CarServiceTests()
        {
            _carRepositoryMock = new Mock<ICarRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CarService>>();
            _service = new CarService(
                _carRepositoryMock.Object,
                _personRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        #region GetAll
        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaPageResponseConDatos()
        {
            // Arrange
            int page = 2;
            int pageSize = 5;
            var cars = new List<Car>
            {
                new Car { Id = 1, PlateNumber = "ABC123", Brand = "Toyota", Model = "Corolla", IsActive = true },
                new Car { Id = 2, PlateNumber = "XYZ789", Brand = "Honda", Model = "Civic", IsActive = true }
            };
            int totalRecords = 10;

            var dtos = new List<CarResponseDto>
            {
                new CarResponseDto { Id = 1, PlateNumber = "ABC123", Brand = "Toyota", Model = "Corolla", IsActive = true },
                new CarResponseDto { Id = 2, PlateNumber = "XYZ789", Brand = "Honda", Model = "Civic", IsActive = true }
            };

            _carRepositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>>()))
                .ReturnsAsync((cars, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<CarResponseDto>>(cars))
                .Returns(dtos);

            // Act
            var result = await _service.GetAllAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(totalRecords, result.TotalRecords);
            Assert.Equal(dtos, result.Data);

            // Verificar llamada al repositorio con los parámetros correctos
            _carRepositoryMock.Verify(r => r.GetPagedAsync(
                page,
                pageSize,
                It.IsAny<Expression<Func<Car, bool>>>(),
                It.IsAny<Expression<Func<Car, object>>>()),
                Times.Once);

            // Verificar que el mapper se llamó con la lista de vehículos
            _mapperMock.Verify(m => m.Map<IEnumerable<CarResponseDto>>(cars), Times.Once);

            // Verificar logs
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Obteniendo vehículos paginados: página {page}, tamaño {pageSize}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Se obtuvieron {dtos.Count} vehículos de un total de {totalRecords}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_SinVehiculos_RetornaPageResponseConListaVacia()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var cars = new List<Car>();
            int totalRecords = 0;

            _carRepositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>>()))
                .ReturnsAsync((cars, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<CarResponseDto>>(cars))
                .Returns(new List<CarResponseDto>());

            // Act
            var result = await _service.GetAllAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(totalRecords, result.TotalRecords);
            Assert.Empty(result.Data);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se obtuvieron 0 vehículos de un total de 0")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_VerificaPredicadoEIncludes()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            _carRepositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>>()))
                .ReturnsAsync((new List<Car>(), 0));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<CarResponseDto>>(It.IsAny<IEnumerable<Car>>()))
                .Returns(new List<CarResponseDto>());

            // Act
            await _service.GetAllAsync(page, pageSize);

            // Assert: verificar predicado e include
            _carRepositoryMock.Verify(r => r.GetPagedAsync(
                page,
                pageSize,
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { IsActive = true }) == true &&
                    expr.Compile().Invoke(new Car { IsActive = false }) == false),
                It.Is<Expression<Func<Car, object>>>(expr =>
                    expr.Body.ToString().Contains("Person") && // Ajustado a "Person"
                    expr.Compile().Invoke(new Car { Persons = new Person() }) != null)),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_CuandoRepositorioLanzaExcepcion_PropagaExcepcion()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var expectedException = new InvalidOperationException("Error en base de datos");

            _carRepositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>>()))
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
            var cars = new List<Car> { new Car { Id = 1, IsActive = true } };
            int totalRecords = 1;
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _carRepositoryMock
                .Setup(r => r.GetPagedAsync(
                    page,
                    pageSize,
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>>()))
                .ReturnsAsync((cars, totalRecords));

            _mapperMock
                .Setup(m => m.Map<IEnumerable<CarResponseDto>>(cars))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.GetAllAsync(page, pageSize));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsCarResponseDto()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678"
            };

            var person = new Person
            {
                Id = 1,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true
            };

            var carEntity = new Car
            {
                Id = 1,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                PersonId = 1,
                IsActive = true
            };

            var responseDto = new CarResponseDto
            {
                Id = 1,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678",
                Nombre = "Juan Pérez",
                IsActive = true
            };

            // Configurar el mock para aceptar cualquier predicado y cualquier array de includes
            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map<Car>(createDto))
                .Returns(carEntity);

            _carRepositoryMock
                .Setup(r => r.AddAsync(carEntity))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(carEntity))
                .Returns(responseDto);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);

            // Verificar que se llamó al repositorio con el predicado correcto (sin importar los includes)
            _personRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { NationalId = createDto.Cedula }) == true),
                It.IsAny<Expression<Func<Person, object>>[]>()), // Usar el tipo de array para params
                Times.Once);

            _mapperMock.Verify(m => m.Map<Car>(createDto), Times.Once);
            _carRepositoryMock.Verify(r => r.AddAsync(carEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(carEntity), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_PersonNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "99999999"
            };

            // Configurar el mock para que devuelva null con la firma correcta (array de includes)
            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync((Person)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal("Persona no encontrada", exception.Message);

            // Verificar que se llamó con el predicado correcto y cualquier array de includes
            _personRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { NationalId = createDto.Cedula }) == true),
                It.IsAny<Expression<Func<Person, object>>[]>()), // Importante: usar [] para el params
                Times.Once);

            // No debe continuar con el mapeo ni la inserción
            _mapperMock.Verify(m => m.Map<Car>(It.IsAny<CreateCarDto>()), Times.Never);
            _carRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(It.IsAny<Car>()), Times.Never);

            // Verificar log de advertencia
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No se encontró persona con cédula {createDto.Cedula} para asociar al vehículo")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenGetPersonThrows_PropagatesException()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678"
            };
            var expectedException = new InvalidOperationException("Error al buscar persona");

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>())) // Cambio aquí
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678"
            };
            var person = new Person { Id = 1 };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(), 
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map<Car>(createDto))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal(expectedException.Message, exception.Message);

            _carRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenAddAsyncThrows_PropagatesException()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678"
            };
            var person = new Person { Id = 1 };
            var carEntity = new Car { PlateNumber = "ABC123", Brand = "Toyota", Model = "Corolla", PersonId = 1 };
            var expectedException = new InvalidOperationException("Error al insertar");

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(), 
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map<Car>(createDto))
                .Returns(carEntity);

            _carRepositoryMock
                .Setup(r => r.AddAsync(carEntity))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal(expectedException.Message, exception.Message);

            // El mapper de respuesta no debe llamarse
            _mapperMock.Verify(m => m.Map<CarResponseDto>(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenPlateNumberAlreadyExists_ThrowsException()
        {
            // Arrange
            var createDto = new CreateCarDto
            {
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678"
            };
            var person = new Person { Id = 1, NationalId = "12345678" };
            var carEntity = new Car { PlateNumber = "ABC123", Brand = "Toyota", Model = "Corolla", PersonId = 1 };
            var duplicateException = new InvalidOperationException("Ya existe un vehículo con esta placa");

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(), 
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map<Car>(createDto))
                .Returns(carEntity);

            _carRepositoryMock
                .Setup(r => r.AddAsync(carEntity))
                .ThrowsAsync(duplicateException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(createDto));

            Assert.Equal("Ya existe un vehículo con esta placa", exception.Message);

            // Verificar que se intentó agregar pero falló
            _carRepositoryMock.Verify(r => r.AddAsync(carEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(It.IsAny<Car>()), Times.Never);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_ValidData_UpdatesCarAndReturnsCarResponseDto()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };

            var existingCar = new Car
            {
                Id = carId,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                PersonId = 2,
                IsActive = true
            };
            var person = new Person
            {
                Id = 1,
                NationalId = "12345678",
                FullName = "Juan Pérez",
                IsActive = true
            };
            var updatedCar = new Car
            {
                Id = carId,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                PersonId = person.Id,
                IsActive = true
            };
            var responseDto = new CarResponseDto
            {
                Id = carId,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = person.NationalId,
                Nombre = person.FullName,
                IsActive = true
            };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingCar))
                .Callback<UpdateCarDto, Car>((dto, car) => { /* simula actualización si hubiera más campos */ })
                .Returns(updatedCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(existingCar))
                .Returns(responseDto);

            // Act
            var result = await _service.UpdateAsync(carId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            Assert.Equal(person.Id, existingCar.PersonId); // Verifica que se actualizó PersonId

            // Verificaciones
            _carRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { Id = carId }) == true),
                It.IsAny<Expression<Func<Car, object>>[]>()),
                Times.Once);

            _personRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Person, bool>>>(expr =>
                    expr.Compile().Invoke(new Person { NationalId = updateDto.Cedula }) == true),
                It.IsAny<Expression<Func<Person, object>>[]>()),
                Times.Once);

            _mapperMock.Verify(m => m.Map(updateDto, existingCar), Times.Once);
            _carRepositoryMock.Verify(r => r.UpdateAsync(existingCar), Times.Once);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(existingCar), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Actualizando vehículo con ID {carId}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con ID {carId} actualizado, ahora asociado a persona {person.Id}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CarNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int carId = 999;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync((Car)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal("Carro no encontrado", exception.Message);

            // No debe buscar persona ni continuar
            _personRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Person, bool>>>(),
                It.IsAny<Expression<Func<Person, object>>[]>()),
                Times.Never);
            _mapperMock.Verify(m => m.Map(It.IsAny<UpdateCarDto>(), It.IsAny<Car>()), Times.Never);
            _carRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);

            // Log de warning
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con ID {carId} no encontrado para actualizar")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_PersonNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "99999999" };
            var existingCar = new Car { Id = carId };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync((Person)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal("Persona no encontrada", exception.Message);

            // No debe continuar con el mapeo ni la actualización
            _mapperMock.Verify(m => m.Map(It.IsAny<UpdateCarDto>(), It.IsAny<Car>()), Times.Never);
            _carRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);

            // Log de warning
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"No se encontró persona con cédula {updateDto.Cedula} para reasignar el vehículo {carId}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenGetCarThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };
            var expectedException = new InvalidOperationException("Error al buscar carro");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenGetPersonThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };
            var existingCar = new Car { Id = carId };
            var expectedException = new InvalidOperationException("Error al buscar persona");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };
            var existingCar = new Car { Id = carId };
            var person = new Person { Id = 1, NationalId = "12345678" };
            var expectedException = new AutoMapperMappingException("Error al mapear");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingCar))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);

            // No debe llegar a UpdateAsync
            _carRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenUpdateCarThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var updateDto = new UpdateCarDto { Cedula = "12345678" };
            var existingCar = new Car { Id = carId };
            var person = new Person { Id = 1, NationalId = "12345678" };
            var expectedException = new InvalidOperationException("Error al actualizar carro");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _personRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Person, bool>>>(),
                    It.IsAny<Expression<Func<Person, object>>[]>()))
                .ReturnsAsync(person);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingCar))
                .Returns(existingCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateAsync(carId, updateDto));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region GetByPlateNumberAsync
        [Fact]
        public async Task GetByPlateNumberAsync_ExistingPlateNumber_ReturnsCarResponseDto()
        {
            // Arrange
            string plateNumber = "ABC123";
            var carEntity = new Car
            {
                Id = 1,
                PlateNumber = plateNumber,
                Brand = "Toyota",
                Model = "Corolla",
                PersonId = 1,
                IsActive = true,
                Persons = new Person { Id = 1, NationalId = "12345678", FullName = "Juan Pérez" }
            };
            var responseDto = new CarResponseDto
            {
                Id = 1,
                PlateNumber = plateNumber,
                Brand = "Toyota",
                Model = "Corolla",
                Cedula = "12345678",
                Nombre = "Juan Pérez",
                IsActive = true
            };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(carEntity);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(carEntity))
                .Returns(responseDto);

            // Act
            var result = await _service.GetByPlateNumberAsync(plateNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);

            _carRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { PlateNumber = plateNumber }) == true &&
                    expr.Compile().Invoke(new Car { PlateNumber = "otra" }) == false),
                It.Is<Expression<Func<Car, object>>[]>(exprs =>
                    exprs.Length == 1 && exprs[0].Body.ToString().Contains("Persons"))),
                Times.Once);

            _mapperMock.Verify(m => m.Map<CarResponseDto>(carEntity), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Buscando vehículo por placa {plateNumber}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con placa {plateNumber} encontrado (ID {carEntity.Id})")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByPlateNumberAsync_NonExistingPlateNumber_ThrowsNotFoundException()
        {
            // Arrange
            string plateNumber = "XYZ999";

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync((Car)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetByPlateNumberAsync(plateNumber));

            Assert.Equal("Carro no encontrado", exception.Message);

            _carRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { PlateNumber = plateNumber }) == true),
                It.IsAny<Expression<Func<Car, object>>[]>()),
                Times.Once);

            _mapperMock.Verify(m => m.Map<CarResponseDto>(It.IsAny<Car>()), Times.Never);

            // Log de warning
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con placa {plateNumber} no encontrado")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByPlateNumberAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            string plateNumber = "ABC123";
            var expectedException = new InvalidOperationException("Error de base de datos");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetByPlateNumberAsync(plateNumber));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task GetByPlateNumberAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            string plateNumber = "ABC123";
            var carEntity = new Car { Id = 1, PlateNumber = plateNumber };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(carEntity);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(carEntity))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.GetByPlateNumberAsync(plateNumber));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region ToggleActive
        [Fact]
        public async Task ToggleActive_ExistingCar_FromActiveToInactive_TogglesAndReturnsDto()
        {
            // Arrange
            int carId = 1;
            var existingCar = new Car
            {
                Id = carId,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                IsActive = true
            };
            var responseDto = new CarResponseDto
            {
                Id = carId,
                PlateNumber = "ABC123",
                Brand = "Toyota",
                Model = "Corolla",
                IsActive = false
            };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(existingCar))
                .Returns(responseDto);

            // Act
            var result = await _service.ToggleActive(carId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            Assert.False(existingCar.IsActive);

            _carRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { Id = carId }) == true),
                It.IsAny<Expression<Func<Car, object>>[]>()),
                Times.Once);

            _carRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Car>(c => c.Id == carId && c.IsActive == false)), Times.Once);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(existingCar), Times.Once);

            // Logs
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Cambiando estado activo de vehículo con ID {carId}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con ID {carId} ahora está inactivo")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_ExistingCar_FromInactiveToActive_TogglesAndReturnsDto()
        {
            // Arrange
            int carId = 2;
            var existingCar = new Car
            {
                Id = carId,
                PlateNumber = "XYZ789",
                Brand = "Honda",
                Model = "Civic",
                IsActive = false
            };
            var responseDto = new CarResponseDto
            {
                Id = carId,
                PlateNumber = "XYZ789",
                Brand = "Honda",
                Model = "Civic",
                IsActive = true
            };

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(existingCar))
                .Returns(responseDto);

            // Act
            var result = await _service.ToggleActive(carId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            Assert.True(existingCar.IsActive);

            _carRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Car>(c => c.Id == carId && c.IsActive == true)), Times.Once);

            // Log con "activo"
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con ID {carId} ahora está activo")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_CarNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int carId = 999;

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync((Car)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.ToggleActive(carId));

            Assert.Equal("Carro no encontrado", exception.Message);

            _carRepositoryMock.Verify(r => r.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Car, bool>>>(expr =>
                    expr.Compile().Invoke(new Car { Id = carId }) == true),
                It.IsAny<Expression<Func<Car, object>>[]>()),
                Times.Once);

            _carRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
            _mapperMock.Verify(m => m.Map<CarResponseDto>(It.IsAny<Car>()), Times.Never);

            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Vehículo con ID {carId} no encontrado para cambiar estado")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ToggleActive_WhenGetFirstOrDefaultThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var expectedException = new InvalidOperationException("Error al buscar vehículo");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ToggleActive(carId));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task ToggleActive_WhenUpdateThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var existingCar = new Car { Id = carId, IsActive = true };
            var expectedException = new InvalidOperationException("Error al actualizar");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ToggleActive(carId));

            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task ToggleActive_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            int carId = 1;
            var existingCar = new Car { Id = carId, IsActive = true };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _carRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Car, bool>>>(),
                    It.IsAny<Expression<Func<Car, object>>[]>()))
                .ReturnsAsync(existingCar);

            _carRepositoryMock
                .Setup(r => r.UpdateAsync(existingCar))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(m => m.Map<CarResponseDto>(existingCar))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _service.ToggleActive(carId));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion



    }
}
